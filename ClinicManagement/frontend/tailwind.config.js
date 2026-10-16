/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        canvas: "#F3F4F7",
        surface: "#FFFFFF",
        ink: {
          DEFAULT: "#12172B",
          muted: "#5B6472",
          faint: "#8B93A1",
        },
        harbor: {
          50: "#EEF2F7",
          100: "#D6E0EC",
          200: "#AEC1D9",
          400: "#5A7BA3",
          500: "#2B4C7E",
          600: "#213C64",
          700: "#182C49",
          900: "#0E1A2C",
        },
        gold: {
          DEFAULT: "#D8B34A",
          dark: "#B8933A",
          light: "#F1E0AC",
        },
        success: { DEFAULT: "#3F8361", light: "#E4F0E9" },
        warning: { DEFAULT: "#C48A2A", light: "#F7EDDA" },
        danger: { DEFAULT: "#B84B3F", light: "#F6E4E1" },
        line: "#E3E5EA",
      },
      fontFamily: {
        display: ["'Newsreader'", "serif"],
        sans: ["'Inter'", "system-ui", "sans-serif"],
        mono: ["'IBM Plex Mono'", "monospace"],
        "display-ar": ["'Noto Kufi Arabic'", "'Cairo'", "sans-serif"],
        "sans-ar": ["'Cairo'", "'Noto Kufi Arabic'", "system-ui", "sans-serif"],
      },
      borderRadius: {
        sm: "6px",
        DEFAULT: "10px",
        lg: "16px",
      },
      boxShadow: {
        card: "0 1px 2px rgba(18, 23, 43, 0.04), 0 4px 16px rgba(18, 23, 43, 0.06)",
      },
    },
  },
  plugins: [],
};
