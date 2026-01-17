<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { useReportsStore } from '../stores/reports';
import { Doughnut, Line } from 'vue-chartjs';
import { 
    Chart as ChartJS, 
    Title, 
    Tooltip, 
    Legend, 
    ArcElement, 
    CategoryScale, 
    LinearScale, 
    PointElement, 
    LineElement,
    Filler
} from 'chart.js';
import { Download, Share2 } from 'lucide-vue-next';

ChartJS.register(Title, Tooltip, Legend, ArcElement, CategoryScale, LinearScale, PointElement, LineElement, Filler);

const reports = useReportsStore();
const currentDate = new Date();
const selectedMonth = ref(currentDate.getMonth() + 1);
const selectedYear = ref(currentDate.getFullYear());

onMounted(async () => {
    await Promise.all([
        reports.fetchExpensesByCategory(selectedMonth.value, selectedYear.value),
        reports.fetchNetWorthEvolution()
    ]);
});

watch([selectedMonth, selectedYear], () => {
    reports.fetchExpensesByCategory(selectedMonth.value, selectedYear.value);
});

// Chart 1: Expenses by Category
const expensesChartData = computed(() => {
    return {
        labels: reports.expensesByCategory.map(item => item.category),
        datasets: [{
            backgroundColor: reports.expensesByCategory.map(item => item.color || '#ccc'),
            data: reports.expensesByCategory.map(item => item.total)
        }]
    };
});

const expensesChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: {
            position: 'right' as const,
            labels: { color: '#e1e1e6', font: { family: 'Inter' } }
        }
    }
};

// Chart 2: Net Worth Evolution
const evolutionChartData = computed(() => {
    return {
        labels: reports.netWorthEvolution.map(item => item.month),
        datasets: [{
            label: 'Patrimônio',
            backgroundColor: (context: any) => {
                const ctx = context.chart.ctx;
                const gradient = ctx.createLinearGradient(0, 0, 0, 400);
                gradient.addColorStop(0, 'rgba(0, 135, 95, 0.5)');
                gradient.addColorStop(1, 'rgba(0, 135, 95, 0.0)');
                return gradient;
            },
            borderColor: '#00875F',
            data: reports.netWorthEvolution.map(item => item.balance),
            fill: true,
            tension: 0.4
        }]
    };
});

const evolutionChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
        y: {
            grid: { color: '#323238' },
            ticks: { 
                color: '#c4c4cc',
                callback: (value: any) => 'R$ ' + value 
            }
        },
        x: {
            grid: { display: false },
            ticks: { color: '#c4c4cc' }
        }
    },
    plugins: {
        legend: { display: false }
    }
};

// Export
const exportReport = () => {
    reports.exportToPDF('report-container', `Relatório Financeiro - ${selectedMonth.value}/${selectedYear.value}`);
};
</script>

<template>
    <div class="p-6 h-full overflow-y-auto space-y-8">
        <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
            <div>
                <h2 class="text-2xl font-bold">Relatórios Avançados</h2>
                <p class="text-gray-400">Análise detalhada da sua vida financeira.</p>
            </div>
            <div class="flex gap-3">
                 <button @click="exportReport" class="flex items-center gap-2 px-4 py-2 bg-[#00875F] hover:bg-[#00B37E] text-white rounded-lg transition-colors">
                    <Download class="w-4 h-4" /> Exportar PDF
                </button>
            </div>
        </div>

        <!-- ID for PDF capture -->
        <div id="report-container" class="space-y-8 bg-[#121214] p-4 rounded-xl">
            
            <!-- Filter for Pie Chart -->
            <div class="flex gap-4">
                <select v-model="selectedMonth" class="bg-[#202024] border border-[#323238] rounded-lg p-2 text-white text-sm">
                    <option v-for="m in 12" :key="m" :value="m">{{ new Date(0, m-1).toLocaleString('pt-BR', { month: 'long' }) }}</option>
                </select>
                <select v-model="selectedYear" class="bg-[#202024] border border-[#323238] rounded-lg p-2 text-white text-sm">
                    <option v-for="y in [2024, 2025, 2026]" :key="y" :value="y">{{ y }}</option>
                </select>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <!-- Expenses Chart -->
                <div class="bg-[#202024] p-6 rounded-2xl border border-[#323238]">
                    <h3 class="text-lg font-bold mb-6">Gastos por Categoria</h3>
                    <div class="h-[300px] flex justify-center">
                        <Doughnut v-if="reports.expensesByCategory.length > 0" :data="expensesChartData" :options="expensesChartOptions" />
                        <div v-else class="flex items-center justify-center h-full text-gray-500">
                            Sem dados para este período.
                        </div>
                    </div>
                </div>

                <!-- Evolution Chart -->
                <div class="bg-[#202024] p-6 rounded-2xl border border-[#323238]">
                    <h3 class="text-lg font-bold mb-6">Evolução Patrimonial (12 Meses)</h3>
                    <div class="h-[300px]">
                        <Line :data="evolutionChartData" :options="evolutionChartOptions" />
                    </div>
                </div>
            </div>

            <!-- Summary Table (Good for PDF) -->
            <div class="mt-8">
                 <h3 class="text-lg font-bold mb-4">Detalhamento de Gastos</h3>
                 <div class="overflow-x-auto">
                    <table class="w-full text-left border-collapse">
                        <thead>
                            <tr class="border-b border-[#323238] text-gray-400 text-sm">
                                <th class="p-4">Categoria</th>
                                <th class="p-4 text-right">Total</th>
                                <th class="p-4 text-right">%</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="item in reports.expensesByCategory" :key="item.category" class="border-b border-[#323238]/50 hover:bg-[#29292E] text-sm">
                                <td class="p-4 flex items-center gap-2">
                                    <div class="w-3 h-3 rounded-full" :style="{ backgroundColor: item.color }"></div>
                                    {{ item.category }}
                                </td>
                                <td class="p-4 text-right">R$ {{ item.total.toFixed(2) }}</td>
                                <td class="p-4 text-right text-gray-500">
                                    {{ ((item.total / reports.expensesByCategory.reduce((a, b) => a + b.total, 0)) * 100).toFixed(1) }}%
                                </td>
                            </tr>
                        </tbody>
                    </table>
                 </div>
            </div>
        </div>
    </div>
</template>
