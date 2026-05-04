import { Paper, Typography, Box } from '@mui/material';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts';

interface StatusChartProps {
  byStatus: Record<string, number>;
}

const STATUS_COLORS: Record<string, string> = {
  Sucesso: '#2e7d32',
  Falha: '#d32f2f',
  Parcial: '#ed6c02',
  EmExecucao: '#1976d2',
  Cancelado: '#757575',
};

export default function StatusChart({ byStatus }: StatusChartProps) {
  const chartData = Object.entries(byStatus).map(([status, count]) => ({
    status,
    count,
    fill: STATUS_COLORS[status] ?? '#9e9e9e',
  }));

  if (chartData.length === 0) return null;

  return (
    <Paper elevation={2} sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom fontWeight="bold" textAlign="center">
        Distribuição por Status
      </Typography>
      <Box sx={{ width: '100%', height: 220 }}>
        <ResponsiveContainer>
          <BarChart data={chartData} margin={{ top: 8, right: 16, left: 0, bottom: 4 }}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} />
            <XAxis dataKey="status" tick={{ fontSize: 13 }} />
            <YAxis allowDecimals={false} tick={{ fontSize: 13 }} />
            <Tooltip
              formatter={(value: number | undefined) => [value ?? 0, 'Execuções'] as [number, string]}
              labelFormatter={(label: unknown) => `Status: ${String(label)}`}
            />
            <Bar dataKey="count" radius={[4, 4, 0, 0]}>
              {chartData.map((entry, index) => (
                <Cell key={index} fill={entry.fill} />
              ))}
            </Bar>
          </BarChart>
        </ResponsiveContainer>
      </Box>
    </Paper>
  );
}
