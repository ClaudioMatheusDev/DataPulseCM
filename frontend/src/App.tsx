import { CssBaseline, ThemeProvider, createTheme, AppBar, Toolbar, Typography, Box } from '@mui/material';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import DashboardPage from './pages/DashboardPage';
import JobDetailsPage from './pages/JobDetailsPage';

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
    background: {
      default: '#f4f6f8',
    },
  },
  typography: {
    fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
  },
});

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <AppBar position="static" elevation={2}>
          <Toolbar>
            <Typography
              variant="h6"
              component={Link}
              to="/"
              sx={{ color: 'white', textDecoration: 'none', fontWeight: 700, letterSpacing: 0.5 }}
            >
              📊 DataPulseCM
            </Typography>
            <Box sx={{ ml: 2, height: 20, borderLeft: '1px solid rgba(255,255,255,0.4)' }} />
            <Typography variant="body2" sx={{ ml: 2, opacity: 0.75 }}>
              Monitoramento de Jobs ETL
            </Typography>
          </Toolbar>
        </AppBar>
        <Routes>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/job/:id" element={<JobDetailsPage />} />
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;