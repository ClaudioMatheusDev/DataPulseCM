import { Chip } from '@mui/material';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ErrorIcon from '@mui/icons-material/Error';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';

interface StatusBadgeProps {
  status: string;
}

export default function StatusBadge({ status }: StatusBadgeProps) {
  // Normalizar status (case-insensitive)
  const normalizedStatus = status?.toUpperCase() || '';
  
  // Determinar configuração baseado no status normalizado
  let config: { color: 'success' | 'error' | 'warning' | 'default'; icon: JSX.Element; label: string };
  
  if (normalizedStatus.includes('SUCESSO') || normalizedStatus === 'SUCCESS') {
    config = {
      color: 'success',
      icon: <CheckCircleIcon fontSize="small" />,
      label: 'Sucesso'
    };
  } else if (normalizedStatus.includes('FALHA') || normalizedStatus.includes('ERRO') || normalizedStatus === 'ERROR' || normalizedStatus === 'FAILED') {
    config = {
      color: 'error',
      icon: <ErrorIcon fontSize="small" />,
      label: 'Falha'
    };
  } else if (normalizedStatus.includes('EXECU') || normalizedStatus.includes('RUNNING') || normalizedStatus.includes('ANDAMENTO')) {
    config = {
      color: 'warning',
      icon: <HourglassEmptyIcon fontSize="small" />,
      label: 'Em Execução'
    };
  } else {
    config = {
      color: 'default',
      icon: <HourglassEmptyIcon fontSize="small" />,
      label: status || 'Desconhecido'
    };
  }

  return <Chip label={config.label} color={config.color} icon={config.icon} size="small" />;
}