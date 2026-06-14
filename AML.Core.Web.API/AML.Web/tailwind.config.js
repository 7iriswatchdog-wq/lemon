/** @type {import('tailwindcss').Config} */
const defaultTheme = require('tailwindcss/defaultTheme');

module.exports = {
  darkMode: 'class',
  content: [
    "./Views/**/*.cshtml",
    "./Pages/**/*.cshtml",
    "./wwwroot/js/**/*.js"
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Geist', ...defaultTheme.fontFamily.sans],
        mono: ['"JetBrains Mono"', ...defaultTheme.fontFamily.mono],
      },
      colors: {
        // Semantic/Brand colors from DESIGN.md
        primary: {
          DEFAULT: '#2563EB', // Royal Blue
          hover: '#1D4ED8',   // Darker Blue
          light: 'rgba(37, 99, 235, 0.12)', // Focus ring
        },
        surface: '#FFFFFF',
        appbg: '#F9FAFB',
        bordercolor: '#D1D5DB',
        // Text
        textprimary: '#111827',
        textmuted: '#6B7280',
        textlabel: '#374151',
        // Status/Risk
        success: '#059669', // Emerald-600
        warning: '#D97706', // Amber-600
        danger: '#DC2626',  // Red-600
        info: '#3B82F6',    // Blue-500
      },
      spacing: {
        '2xs': '2px',
        'xs': '4px',
        'sm': '8px',
        'md': '16px',
        'lg': '24px',
        'xl': '32px',
        '2xl': '48px',
        '3xl': '64px',
      },
      boxShadow: {
        'focus-ring': '0 0 0 3px rgba(37, 99, 235, 0.12)',
      }
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
    require('@tailwindcss/aspect-ratio'),
    require('@tailwindcss/container-queries'),
  ],
}
