<script setup lang="ts">
import { computed } from 'vue';
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

const props = defineProps<{
  chartData: {
    labels: string[];
    incomeData: number[];
    expenseData: number[];
  }
}>();

const data = computed(() => ({
  labels: props.chartData?.labels || [],
  datasets: [
    {
      label: 'Entradas',
      backgroundColor: 'rgba(59, 130, 246, 0.2)', // Blue
      borderColor: '#3b82f6',
      data: props.chartData?.incomeData || [],
      fill: true,
      tension: 0.4
    },
    {
      label: 'Saídas',
      backgroundColor: 'rgba(239, 68, 68, 0.2)', // Red
      borderColor: '#ef4444',
      data: props.chartData?.expenseData || [],
      fill: true,
      tension: 0.4
    }
  ]
}));

const options = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    tooltip: {
      callbacks: {
        label: (context: any) => {
          let label = context.dataset.label || '';
          if (label) {
            label += ': ';
          }
          if (context.parsed.y !== null) {
            label += new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(context.parsed.y);
          }
          return label;
        }
      }
    },
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
      ticks: { 
        color: '#94a3b8',
        callback: (value: any) => {
          return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL', maximumSignificantDigits: 3 }).format(value);
        }
      }
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
