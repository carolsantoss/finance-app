/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Outfit', 'Inter', 'sans-serif'],
      },
      colors: {
        // Semantic Colors mapped to CSS Variables
        app: 'var(--bg-app)',
        card: 'var(--bg-card)',
        input: 'var(--bg-input)',
        hover: 'var(--bg-hover)',
        border: 'var(--border-color)',
        
        'text-primary': 'var(--text-primary)',
        'text-secondary': 'var(--text-secondary)',
        'text-tertiary': 'var(--text-tertiary)',
        
        brand: {
             DEFAULT: 'var(--brand-primary)',
             hover: 'var(--brand-secondary)'
        },
        
        danger: 'var(--danger)',

        // Keep existing legacy if needed, or remove? 
        // Removing old hardcoded primary/dark to encourage using new semantic ones.
        primary: {
          50: '#f0f9ff',
          100: '#e0f2fe',
          500: '#0ea5e9',
          600: '#0284c7',
          900: '#0c4a6e',
        },
        dark: {
          800: '#1e293b',
          900: '#0f172a',
        }
      },
      backdropBlur: {
        xs: '2px',
      }
    },
  },
  plugins: [],
}
