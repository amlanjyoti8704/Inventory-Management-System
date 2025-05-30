// components/DarkModeToggle.jsx
import { useEffect, useState } from "react";

function DarkModeToggle() {
  const [isDark, setIsDark] = useState(() => {
    return localStorage.getItem("theme") === "dark";
  });

  useEffect(() => {
    const root = document.documentElement;
    if (isDark) {
      root.classList.add("dark");
      localStorage.setItem("theme", "dark");
    } else {
      root.classList.remove("dark");
      localStorage.setItem("theme", "light");
    }
  }, [isDark]);

  const toggleTheme = () => {
    setIsDark((prev) => !prev);
  };

  return (
    <button
      onClick={toggleTheme}
      className="ml-4 text-white bg-gray-800 dark:bg-yellow-300 dark:text-black px-3 py-1 rounded"
    >
      {isDark ? "Light Mode" : "Dark Mode"}
    </button>
  );
}

export default DarkModeToggle;