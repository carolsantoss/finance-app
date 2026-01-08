<script setup lang="ts">
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
} from 'chart.js'
import { Line } from 'vue-chartjs'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
)

const data = {
  labels: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul'],
  datasets: [
    {
      label: 'Entradas',
      backgroundColor: 'rgba(59, 130, 246, 0.2)', // Blue
      borderColor: '#3b82f6',
      data: [4000, 3000, 5000, 4500, 6000, 5500, 7000],
      fill: true,
      tension: 0.4
    },
    {
      label: 'Saídas',
      backgroundColor: 'rgba(239, 68, 68, 0.2)', // Red
      borderColor: '#ef4444',
      data: [2500, 2000, 3500, 3000, 4000, 3500, 4500],
      fill: true,
      tension: 0.4
    }
  ]
}

const options = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'top' as const,
      labels: {
        color: '#94a3b8' // Slate 400
      }
    }
  },
  scales: {
    y: {
      grid: { color: '#334155' }, // Slate 700
      ticks: { color: '#94a3b8' }
    },
    x: {
      grid: { display: false },
      ticks: { color: '#94a3b8' }
    }
  }
}
</script>

<template>
  <div class="w-full h-[300px]">
    <Line :data="data" :options="options" />
  </div>
</template>
