/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './**/*.razor',
    './**/*.html',
    './Pages/**/*.razor',
    './Components/**/*.razor',
    './Layouts/**/*.razor',
    './Shared/**/*.razor'
  ],
  theme: {
    extend: {
      colors: {
        // Design Industrial Premium Palette
        primary: {
          DEFAULT: '#00327d',
          dark: '#002461',
          light: '#1a4a9a'
        },
        surface: {
          DEFAULT: '#ffffff',
          dim: '#f5f5f5',
          bright: '#ffffff',
          container: '#f8f9fa',
          'container-low': '#fafbfc',
          'container-high': '#e9ecef'
        },
        'on-surface': {
          DEFAULT: '#1c1b1f',
          variant: '#49454f'
        },
        outline: {
          DEFAULT: '#79747e',
          variant: '#cac4d0'
        },
        success: '#4caf50',
        warning: '#ff9800',
        error: '#f44336',
        info: '#2196f3'
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        headline: ['Space Grotesk', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'monospace']
      },
      spacing: {
        safe: 'env(safe-area-inset-bottom)',
        'safe-top': 'env(safe-area-inset-top)',
        'safe-left': 'env(safe-area-inset-left)',
        'safe-right': 'env(safe-area-inset-right)'
      },
      borderRadius: {
        sm: '4px',
        DEFAULT: '8px',
        md: '12px',
        lg: '16px',
        xl: '24px'
      },
      keyframes: {
        scan: {
          '0%': { top: '0%' },
          '50%': { top: '100%' },
          '100%': { top: '0%' }
        }
      },
      animation: {
        scan: 'scan 2s ease-in-out infinite'
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms')
  ]
}
