import type { ReactElement } from 'react';
import { Chip } from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import CancelIcon from '@mui/icons-material/Cancel';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';

interface StatusBadgeProps {
  status: string;
}

export default function StatusBadge({ status }: StatusBadgeProps) {
  const normalizedStatus = status?.toUpperCase() || '';

  let config: { color: 'success' | 'error' | 'warning' | 'info' | 'default'; icon: ReactElement; label: string };

  if (normalizedStatus.includes('SUCESSO') || normalizedStatus === 'SUCCESS') {
    config = { color: 'success', icon: <CheckCircleIcon fontSize="small" />, label: 'Sucesso' };
  } else if (normalizedStatus.includes('FALHA') || normalizedStatus.includes('ERRO') || normalizedStatus === 'ERROR' || normalizedStatus === 'FAILED') {
    config = { color: 'error', icon: <ErrorIcon fontSize="small" />, label: 'Falha' };
  } else if (normalizedStatus.includes('PARCIAL')) {
    config = { color: 'warning', icon: <WarningAmberIcon fontSize="small" />, label: 'Parcial' };
  } else if (normalizedStatus.includes('CANCELADO') || normalizedStatus === 'CANCELLED' || normalizedStatus === 'CANCELED') {
    config = { color: 'default', icon: <CancelIcon fontSize="small" />, label: 'Cancelado' };
  } else if (normalizedStatus.includes('EXECU') || normalizedStatus.includes('RUNNING') || normalizedStatus.includes('ANDAMENTO')) {
    config = { color: 'info', icon: <HourglassEmptyIcon fontSize="small" />, label: 'Em Execução' };
  } else {
    config = { color: 'default', icon: <HourglassEmptyIcon fontSize="small" />, label: status || 'Desconhecido' };
  }

  return <Chip label={config.label} color={config.color} icon={config.icon} size="small" />;
}