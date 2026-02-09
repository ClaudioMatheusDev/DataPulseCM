import { 
  Table, 
  TableBody, 
  TableCell, 
  TableContainer, 
  TableHead, 
  TableRow, 
  Paper,
  Typography,
  Chip,
} from '@mui/material';
import { useNavigate } from 'react-router-dom';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import StatusBadge from '../Common/StatusBadge';
import type { JobExecution } from '../../types/job.types';

interface JobsTableProps {
  jobs: JobExecution[];
}

export default function JobsTable({ jobs }: JobsTableProps) {
  const navigate = useNavigate();

  const calculateDuration = (startDate?: string, endDate?: string) => {
    if (!startDate || !endDate) return null;
    try {
      const start = new Date(startDate);
      const end = new Date(endDate);
      if (start.getFullYear() === 1 || end.getFullYear() === 1) return null;
      const diffMs = end.getTime() - start.getTime();
      return Math.floor(diffMs / 1000); // segundos
    } catch {
      return null;
    }
  };

  const formatDuration = (duration?: number) => {
    if (!duration || duration <= 0) return '-';
    if (duration < 60) return `${duration}s`;
    const minutes = Math.floor(duration / 60);
    const seconds = duration % 60;
    return `${minutes}min ${seconds}s`;
  };

  const formatDate = (dateString: string) => {
    try {
      // Verificar se é data válida (não '0001-01-01')
      const date = new Date(dateString);
      if (date.getFullYear() === 1 || dateString.startsWith('0001')) {
        return '-';
      }
      return format(date, "dd/MM/yyyy HH:mm:ss", { locale: ptBR });
    } catch {
      return '-';
    }
  };

  return (
    <TableContainer component={Paper} elevation={2}>
      <Table>
        <TableHead>
          <TableRow sx={{ backgroundColor: '#f5f5f5' }}>
            <TableCell><strong>ID</strong></TableCell>
            <TableCell><strong>Nome do Job</strong></TableCell>
            <TableCell><strong>Status</strong></TableCell>
            <TableCell><strong>Data Início</strong></TableCell>
            <TableCell><strong>Duração</strong></TableCell>
            <TableCell><strong>Registros</strong></TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {jobs.length === 0 ? (
            <TableRow>
              <TableCell colSpan={6} align="center">
                <Typography color="textSecondary">Nenhuma execução encontrada</Typography>
              </TableCell>
            </TableRow>
          ) : (
            jobs.map((job) => (
              <TableRow 
                key={job.executionID}
                hover
                onClick={() => navigate(`/job/${job.executionID}`)}
                sx={{ 
                  '&:hover': { backgroundColor: '#f5f5f5', cursor: 'pointer' },
                  transition: 'background-color 0.2s'
                }}
              >
                <TableCell>{job.executionID}</TableCell>
                <TableCell>
                  <Typography variant="body2" fontWeight="medium">
                    {job.jobName}
                  </Typography>
                </TableCell>
                <TableCell>
                  <StatusBadge status={job.status} />
                </TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {formatDate(job.startDate)}
                  </Typography>
                  {job.endDate && formatDate(job.endDate) !== '-' && (
                    <Typography variant="caption" color="textSecondary" display="block">
                      até {formatDate(job.endDate).split(' ')[0]}
                    </Typography>
                  )}
                </TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {formatDuration(job.duration || calculateDuration(job.startDate, job.endDate) || 0)}
                  </Typography>
                </TableCell>
                <TableCell>
                  {job.recordsProcessed && job.recordsProcessed > 0 ? (
                    <Chip label={job.recordsProcessed.toLocaleString()} size="small" variant="outlined" />
                  ) : (
                    <Typography variant="body2" color="textSecondary">-</Typography>
                  )}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );
}