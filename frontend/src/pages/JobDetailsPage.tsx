/* eslint-disable @typescript-eslint/no-unused-vars */
import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Paper,
  Button,
  Alert,
  Divider,
  Chip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import StatusBadge from '../components/Common/StatusBadge';
import Loading from '../components/Common/Loading';
import { jobsApi } from '../services/api';
import type { JobExecution, JobExecutionDetail } from '../types/job.types';

export default function JobDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [job, setJob] = useState<JobExecution | null>(null);
  const [details, setDetails] = useState<JobExecutionDetail[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadJobDetails();
  }, [id]);

  const loadJobDetails = async () => {
    if (!id) return;

    try {
      setLoading(true);
      setError(null);

      const [jobData, detailsData] = await Promise.all([
        jobsApi.getJobById(Number(id)),
        jobsApi.getJobDetails(Number(id)),
      ]);

      setJob(jobData);
      setDetails(detailsData.steps || []);
    } catch (err: unknown) {
      console.error('Erro ao carregar detalhes:', err);
      setError('Falha ao carregar detalhes da execução.');
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      if (date.getFullYear() === 1 || dateString.startsWith('0001')) {
        return '-';
      }
      return format(date, "dd/MM/yyyy 'às' HH:mm:ss", { locale: ptBR });
    } catch {
      return '-';
    }
  };

  const formatDuration = (duration?: number) => {
    if (!duration) return '-';
    if (duration < 60) return `${duration}s`;
    const minutes = Math.floor(duration / 60);
    const seconds = duration % 60;
    return `${minutes}min ${seconds}s`;
  };

  const calculateDuration = (start?: string, end?: string) => {
    if (!start || !end) return null;
    try {
      const startDate = new Date(start);
      const endDate = new Date(end);
      const diffMs = endDate.getTime() - startDate.getTime();
      return Math.floor(diffMs / 1000); // segundos
    } catch {
      return null;
    }
  };

  if (loading) return <Loading />;

  if (error || !job) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Alert severity="error">{error || 'Execução não encontrada'}</Alert>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/')}
          sx={{ mt: 2 }}
        >
          Voltar ao Dashboard
        </Button>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4, px: { xs: 2, sm: 3, md: 4 } }}>
      {/* Botão Voltar */}
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => navigate('/')}
        sx={{ mb: 3 }}
        variant="outlined"
      >
        Voltar ao Dashboard
      </Button>

      {/* Cabeçalho */}
      <Paper elevation={2} sx={{ p: { xs: 2, sm: 3, md: 4 }, mb: 3 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center" flexWrap="wrap" gap={2} mb={2}>
          <Box>
            <Typography variant="h4" component="h1" fontWeight="bold">
              {job.jobName}
            </Typography>
            <Typography variant="caption" color="textSecondary">
              Execução ID: #{job.executionId}
            </Typography>
          </Box>
          <StatusBadge status={job.status} />
        </Box>

        <Divider sx={{ my: 2 }} />

        {/* Informações Gerais */}
        <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 4 }}>
          {formatDate(job.startDateTime) !== '-' && (
            <Box>
              <Typography variant="caption" color="textSecondary" display="block">
                Data de Início
              </Typography>
              <Typography variant="body1" fontWeight="medium">
                <AccessTimeIcon sx={{ fontSize: 16, mr: 0.5, verticalAlign: 'middle' }} />
                {formatDate(job.startDateTime)}
              </Typography>
            </Box>
          )}

          {job.endDateTime && formatDate(job.endDateTime) !== '-' && (
            <Box>
              <Typography variant="caption" color="textSecondary" display="block">
                Data de Término
              </Typography>
              <Typography variant="body1" fontWeight="medium">
                {formatDate(job.endDateTime)}
              </Typography>
            </Box>
          )}

          {(job.executionDurationMs || calculateDuration(job.startDateTime, job.endDateTime)) && (
            <Box>
              <Typography variant="caption" color="textSecondary" display="block">
                Duração Total
              </Typography>
              <Typography variant="body1" fontWeight="medium">
                {job.executionDurationMs 
                  ? formatDuration(Math.floor(job.executionDurationMs / 1000))
                  : formatDuration(calculateDuration(job.startDateTime, job.endDateTime) || 0)}
              </Typography>
            </Box>
          )}

          {job.rowsProcessed !== undefined && job.rowsProcessed !== null && job.rowsProcessed > 0 && (
            <Box>
              <Typography variant="caption" color="textSecondary" display="block">
                Registros Processados
              </Typography>
              <Typography variant="body1" fontWeight="medium">
                {job.rowsProcessed.toLocaleString()}
              </Typography>
            </Box>
          )}

          {job.rowsInserted !== undefined && job.rowsInserted !== null && job.rowsInserted > 0 && (
            <Box>
              <Typography variant="caption" color="textSecondary" display="block">
                Inseridos
              </Typography>
              <Typography variant="body1" fontWeight="medium" color="success.main">
                {job.rowsInserted.toLocaleString()}
              </Typography>
            </Box>
          )}

          {job.rowsUpdated !== undefined && job.rowsUpdated !== null && job.rowsUpdated > 0 && (
            <Box>
              <Typography variant="caption" color="textSecondary" display="block">
                Atualizados
              </Typography>
              <Typography variant="body1" fontWeight="medium" color="primary.main">
                {job.rowsUpdated.toLocaleString()}
              </Typography>
            </Box>
          )}
        </Box>

        {/* Informação adicional quando não há dados de execução */}
        {formatDate(job.startDateTime) === '-' && (
          <Alert severity="info" sx={{ mt: 2 }}>
            Esta execução ainda não possui informações de data e hora registradas.
          </Alert>
        )}

        {/* Mensagem de Erro */}
        {job.errorMessage && job.errorMessage.toLowerCase() !== 'null' && (
          <Alert severity="error" sx={{ mt: 3 }}>
            <Typography variant="subtitle2" fontWeight="bold">
              Mensagem de Erro:
            </Typography>
            <Typography variant="body2">{job.errorMessage}</Typography>
          </Alert>
        )}
      </Paper>

      {/* Steps de Execução */}
      <Paper elevation={2} sx={{ p: { xs: 2, sm: 3, md: 4 } }}>
        <Typography variant="h5" gutterBottom fontWeight="bold">
          Detalhes de Execução
        </Typography>
        <Typography variant="body2" color="textSecondary" gutterBottom>
          {details.length > 0 
            ? `${details.length} step(s) registrado(s)` 
            : 'Nenhum step detalhado foi registrado para esta execução'}
        </Typography>

        {details.length === 0 ? (
          <Box
            sx={{
              mt: 3,
              p: 4,
              textAlign: 'center',
              backgroundColor: '#f9f9f9',
              borderRadius: 2,
            }}
          >
            <Typography variant="h6" color="textSecondary" gutterBottom>
              📋 Sem steps detalhados
            </Typography>
            <Typography variant="body2" color="textSecondary">
              Esta execução não possui steps individuais rastreados.
              {job.status === 'Sucesso' 
                ? ' A execução foi concluída com sucesso.' 
                : ' Verifique os logs para mais informações.'}
            </Typography>
          </Box>
        ) : (
          <TableContainer sx={{ mt: 2 }}>
            <Table>
              <TableHead>
                <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
                  <TableCell><strong>Ordem</strong></TableCell>
                  <TableCell><strong>Nome do Step</strong></TableCell>
                  <TableCell><strong>Status</strong></TableCell>
                  <TableCell><strong>Duração</strong></TableCell>
                  <TableCell align="right"><strong>Processados</strong></TableCell>
                  <TableCell align="right"><strong>Inseridos</strong></TableCell>
                  <TableCell align="right"><strong>Atualizados</strong></TableCell>
                  <TableCell align="center"><strong>Progresso</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {details
                  .sort((a, b) => a.stepOrder - b.stepOrder)
                  .map((detail) => (
                    <TableRow key={detail.detailId} hover>
                      <TableCell>
                        <Chip label={detail.stepOrder} size="small" variant="outlined" />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" fontWeight="medium">
                          {detail.stepName}
                        </Typography>
                        {detail.stepMessage && detail.stepStatus === 'Falha' && (
                          <Typography variant="caption" color="error" display="block" sx={{ mt: 0.5 }}>
                            ⚠️ {detail.stepMessage}
                          </Typography>
                        )}
                        {detail.stepMessage && detail.stepStatus !== 'Falha' && (
                          <Typography variant="caption" color="textSecondary" display="block" sx={{ mt: 0.5 }}>
                            {detail.stepMessage}
                          </Typography>
                        )}
                        <Typography variant="caption" color="textSecondary" display="block" sx={{ mt: 0.5 }}>
                          {formatDate(detail.startDateTime)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <StatusBadge status={detail.stepStatus} />
                      </TableCell>
                      <TableCell>
                        {detail.durationInSeconds !== undefined && detail.durationInSeconds !== null
                          ? formatDuration(Math.floor(detail.durationInSeconds))
                          : calculateDuration(detail.startDateTime, detail.endDateTime)
                          ? formatDuration(calculateDuration(detail.startDateTime, detail.endDateTime) || 0)
                          : '-'}
                      </TableCell>
                      <TableCell align="right">
                        {detail.rowsProcessed ? (
                          <Chip 
                            label={detail.rowsProcessed.toLocaleString()} 
                            size="small" 
                            color="default"
                          />
                        ) : (
                          '-'
                        )}
                      </TableCell>
                      <TableCell align="right">
                        {detail.rowsInserted ? (
                          <Chip 
                            label={detail.rowsInserted.toLocaleString()} 
                            size="small" 
                            color="success"
                          />
                        ) : (
                          '-'
                        )}
                      </TableCell>
                      <TableCell align="right">
                        {detail.rowsUpdated ? (
                          <Chip 
                            label={detail.rowsUpdated.toLocaleString()} 
                            size="small" 
                            color="primary"
                          />
                        ) : (
                          '-'
                        )}
                      </TableCell>
                      <TableCell align="center">
                        {detail.progressPercentage !== undefined && detail.progressPercentage !== null ? (
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Box
                              sx={{
                                width: 50,
                                height: 8,
                                backgroundColor: '#e0e0e0',
                                borderRadius: 1,
                                overflow: 'hidden',
                              }}
                            >
                              <Box
                                sx={{
                                  width: `${detail.progressPercentage}%`,
                                  height: '100%',
                                  backgroundColor: detail.progressPercentage === 100 ? '#4caf50' : '#2196f3',
                                  transition: 'width 0.3s',
                                }}
                              />
                            </Box>
                            <Typography variant="caption" sx={{ minWidth: 35 }}>
                              {detail.progressPercentage.toFixed(0)}%
                            </Typography>
                          </Box>
                        ) : (
                          '-'
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
              </TableBody>
            </Table>
          </TableContainer>
        )}
      </Paper>
    </Container>
  );
}
