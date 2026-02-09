import { useState, useEffect } from 'react';
import { Container, Typography, Box, Alert } from '@mui/material';
import StatisticsCards from '../components/DashBoard/StatisticsCards';
import JobsTable from '../components/DashBoard/JobsTable';
import Loading from '../components/Common/Loading';
import { jobsApi } from '../services/api';
import type { JobExecution, Statistics } from '../types/job.types';

export default function DashboardPage() {
  const [statistics, setStatistics] = useState<Statistics | null>(null);
  const [jobs, setJobs] = useState<JobExecution[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadDashboardData();
    // Atualizar a cada 30 segundos
    const interval = setInterval(loadDashboardData, 30000);
    return () => clearInterval(interval);
  }, []);

  const loadDashboardData = async () => {
    try {
      setError(null);
      
      const [statsData, jobsData] = await Promise.all([
        jobsApi.getStatistics(),
        jobsApi.getRecentJobs(50),
      ]);
      
      setStatistics(statsData);
      setJobs(jobsData.data);
    } catch (err: unknown) {
      console.error('Erro ao carregar dados:', err);
      
      let errorMessage = 'Falha ao carregar dados do dashboard.';
      
      if (err.code === 'ERR_NETWORK') {
        errorMessage = 'Erro de rede. Verifique se a API está rodando em http://localhost:5105';
      } else if (err.response) {
        errorMessage = `Erro na API: ${err.response.status} - ${err.response.statusText}`;
      }
      
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <Loading />;

  if (error) {
    return (
      <Container maxWidth="lg" sx={{ mt: 4 }}>
        <Alert severity="error">{error}</Alert>
      </Container>
    );
  }

  return (
    <Container maxWidth="md" sx={{ mt: 4, mb: 4, px: { xs: 2, sm: 3, md: 4 } }}>
      <Typography variant="h4" component="h1" gutterBottom fontWeight="bold" textAlign="center">
        📊 Dashboard - DataPulseCM
      </Typography>
      <Typography variant="subtitle1" color="textSecondary" gutterBottom textAlign="center" sx={{ mb: 4 }}>
        Monitoramento de Execuções de Jobs ETL
      </Typography>

      {statistics && (
        <Box sx={{ mb: 4 }}>
          <StatisticsCards statistics={statistics} />
        </Box>
      )}

      <Box>
        <Typography variant="h5" gutterBottom sx={{ mb: 2, mt: 2 }} textAlign="center">
          Execuções Recentes
        </Typography>
        <JobsTable jobs={jobs} />
      </Box>
    </Container>
  );
}