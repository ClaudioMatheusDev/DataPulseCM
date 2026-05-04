import { useState, useEffect } from 'react';
import { Container, Typography, Box, Alert } from '@mui/material';
import StatisticsCards from '../components/DashBoard/StatisticsCards';
import JobsTable from '../components/DashBoard/JobsTable';
import StatusChart from '../components/DashBoard/StatusChart';
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

      if (err && typeof err === 'object' && 'code' in err && err.code === 'ERR_NETWORK') {
        errorMessage = 'Erro de rede. Verifique se a API está rodando em http://localhost:5105';
      } else if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response: { status: number; statusText: string } };
        errorMessage = `Erro na API: ${axiosErr.response.status} - ${axiosErr.response.statusText}`;
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
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4, px: { xs: 2, sm: 3 } }}>

      {statistics && (
        <Box sx={{ mb: 4 }}>
          <StatisticsCards statistics={statistics} />
        </Box>
      )}

      {statistics?.byStatus && Object.keys(statistics.byStatus).length > 0 && (
        <Box sx={{ mb: 4 }}>
          <StatusChart byStatus={statistics.byStatus} />
        </Box>
      )}

      <Box>
        <Typography variant="h6" gutterBottom sx={{ mb: 2, mt: 2, fontWeight: 'bold' }}>
          Execuções Recentes
        </Typography>
        <JobsTable jobs={jobs} />
      </Box>
    </Container>
  );
}