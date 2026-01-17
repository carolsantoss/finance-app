import { defineStore } from 'pinia';
import api from '../api/axios';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import html2canvas from 'html2canvas';

export const useReportsStore = defineStore('reports', {
    state: () => ({
        expensesByCategory: [] as any[],
        netWorthEvolution: [] as any[],
        isLoading: false
    }),
    actions: {
        async fetchExpensesByCategory(month: number, year: number) {
            try {
                const response = await api.get('/reports/expenses-by-category', { params: { month, year } });
                this.expensesByCategory = response.data;
            } catch (error) {
                console.error(error);
            }
        },
        async fetchNetWorthEvolution() {
            try {
                const response = await api.get('/reports/net-worth-evolution');
                this.netWorthEvolution = response.data;
            } catch (error) {
                console.error(error);
            }
        },
        async exportToPDF(elementId: string, title: string = 'Relatório Financeiro') {
            const element = document.getElementById(elementId);
            if (!element) return;

            // Capture the element as an image (preserve styling/beauty)
            // Use temporary style to ensure it renders correctly for print
            const canvas = await html2canvas(element, {
                scale: 2,
                backgroundColor: '#121214' // Match theme
            });
            const imgData = canvas.toDataURL('image/png');

            const pdf = new jsPDF('p', 'mm', 'a4');
            const pageWidth = pdf.internal.pageSize.getWidth();
            const pageHeight = pdf.internal.pageSize.getHeight();

            // Add Logo/Header
            pdf.setFillColor(0, 135, 95); // Brand Green
            pdf.rect(0, 0, pageWidth, 20, 'F');
            pdf.setTextColor(255, 255, 255);
            pdf.setFontSize(16);
            pdf.text('Finance App Report', 10, 12);
            pdf.setFontSize(10);
            pdf.text(new Date().toLocaleDateString('pt-BR'), pageWidth - 30, 12);

            // Add the Chart/Content Image
            const imgWidth = pageWidth - 20;
            const imgHeight = (canvas.height * imgWidth) / canvas.width;

            pdf.addImage(imgData, 'PNG', 10, 30, imgWidth, imgHeight);

            // Save
            pdf.save(`finance-report-${new Date().toISOString().split('T')[0]}.pdf`);
        }
    }
});
