import MockAdapter from 'axios-mock-adapter';
import axios from 'axios';

export const setupMock = (axiosInstance: any) => {
    const mock = new MockAdapter(axiosInstance, { delayResponse: 500 });

    // Auth Mocks
    mock.onPost('/auth/login').reply(200, {
        token: 'mock-jwt-token-123',
        nomeUsuario: 'Carol Santos'
    });

    mock.onPost('/auth/register').reply(200, {
        token: 'mock-jwt-token-123',
        nomeUsuario: 'Novo Usuário'
    });

    // Transactions Mocks
    const transactions = [
        { id_lancamento: 1, nm_tipo: 'Entrada', nm_descricao: 'Salário Mensal', nr_valor: 5000.00, dt_dataLancamento: '2025-01-05' },
        { id_lancamento: 2, nm_tipo: 'Saída', nm_descricao: 'Aluguel', nr_valor: 1200.00, dt_dataLancamento: '2025-01-10' },
        { id_lancamento: 3, nm_tipo: 'Saída', nm_descricao: 'Supermercado', nr_valor: 450.50, dt_dataLancamento: '2025-01-15' },
        { id_lancamento: 4, nm_tipo: 'Entrada', nm_descricao: 'Freelance Design', nr_valor: 1500.00, dt_dataLancamento: '2025-01-20' },
    ];

    mock.onGet('/lancamentos').reply(200, transactions);

    mock.onGet('/lancamentos/summary').reply(200, {
        entradas: 6500.00,
        saidas: 1650.50,
        saldo: 4849.50
    });

    mock.onPost('/lancamentos').reply(config => {
        const data = JSON.parse(config.data);
        const newItem = {
            ...data,
            id_lancamento: Math.floor(Math.random() * 1000)
        };
        transactions.push(newItem);
        return [201, newItem];
    });

    mock.onDelete(/\/lancamentos\/\d+/).reply(200);

    console.log('✅ Mock Adapter initialized: Running in standalone mode.');
};
