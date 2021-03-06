module.exports = {
  mode: 'jit',
  purge: ['./index.html', './src/**/*.{js,jsx}', './node_modules/@agrc/dart-board/**/*.js'],
  darkMode: false, // or 'media' or 'class'
  theme: {
    minHeight: {
      profile: '36em',
    },
    extend: {},
  },
  variants: {
    extend: {},
  },
  plugins: [require('@tailwindcss/forms')],
};
