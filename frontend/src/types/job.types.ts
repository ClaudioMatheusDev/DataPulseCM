export interface JobExecution {
  executionID: number;
  jobName: string;
  status: string;
  startDate: string;
  endDate?: string;
  duration?: number;
  errorMessage?: string;
  recordsProcessed?: number;
}

export interface Statistics {
  total: number;
  successful: number;
  failed: number;
  successRate: number;
  byStatus?: unknown[];
  period?: {
    startDate?: string;
    endDate?: string;
  };
}

export interface JobExecutionDetail {
  detailId: number;
  executionId: number;
  stepName: string;
  stepOrder: number;
  status: string;
  startTime: string;
  endTime?: string;
  recordsProcessed?: number;
  errorMessage?: string;
  stepMessage?: string;
}