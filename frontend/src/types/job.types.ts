export interface JobExecution {
  executionId: number;
  jobName: string;
  status: string;
  startDateTime: string;
  endDateTime?: string;
  executionDurationMs?: number;
  errorMessage?: string;
  rowsProcessed?: number;
  rowsInserted?: number;
  rowsUpdated?: number;
  rowsDeleted?: number;
  serverName?: string;
  databaseName?: string;
  createdAt?: string;
}

export interface Statistics {
  total: number;
  successful: number;
  failed: number;
  successRate: number;
  byStatus?: Record<string, number>;
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
  stepStatus: string;
  stepMessage?: string;
  startDateTime: string;
  endDateTime?: string;
  rowsProcessed?: number;
  rowsInserted?: number;
  rowsUpdated?: number;
  rowsDeleted?: number;
  rowsFailed?: number;
  progressPercentage?: number;
  durationInSeconds?: number;
  durationInMilliseconds?: number;
  createdAt?: string;
}