import axios from 'axios';
import type { JobExecution, Statistics, JobExecutionDetail } from '../types/job.types';

const API_BASE = 'http://localhost:5105/api';

const api = axios.create({
  baseURL: API_BASE,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const jobsApi = {
  getRecentJobs: async (limit = 50) => {
    const { data } = await api.get<{ data: JobExecution[]; count: number }>(`/jobs?limit=${limit}`);
    return data;
  },

  getStatistics: async () => {
    const { data } = await api.get<Statistics>('/jobs/statistics');
    return data;
  },

  getFailedJobs: async () => {
    const { data } = await api.get<JobExecution[]>('/jobs/failed');
    return data;
  },

  getJobById: async (id: number) => {
    const { data } = await api.get<JobExecution>(`/jobs/${id}`);
    return data;
  },

  getJobDetails: async (id: number) => {
    const { data } = await api.get<{ executionId: number; steps: JobExecutionDetail[] }>(`/jobs/${id}/details`);
    return data;
  },

  startJob: async (jobName: string) => {
    const { data } = await api.post('/jobs/start', { jobName });
    return data;
  },

  finishJob: async (id: number, status: string, errorMessage?: string) => {
    const { data } = await api.post(`/jobs/${id}/finish`, { status, errorMessage });
    return data;
  },
};