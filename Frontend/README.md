# React + Vite

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Babel](https://babeljs.io/) for Fast Refresh
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/) for Fast Refresh

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.

# 📦 Inventory Management System

A full-stack Inventory Management System designed to manage IT consumable items efficiently. It features item categorization, stock tracking, supplier & purchase management, and issue records — powered by a modern tech stack.

---

## 🛠 Tech Stack

- **Frontend**: Tailwind, React
- **Backend**: C# (.NET Core)
- **Database**: MySQL
- **Dev Tools**: VS Code, SQLyog, Git, Terminal (macOS)

---

## 📁 Project Structure

```bash
testProject/
├── backend/                 # C# backend source code
│   ├── Controllers/
│   ├── Models/
│   ├── Program.cs
│   ├── appsettings.json
│   └── ...
├── frontend/                # Frontend (HTML/CSS/JS or React)
│   ├── index.html
│   ├── styles.css
│   └── script.js
├── Database/                # Database export and schema files
│   └── IT_consumables.sql
├── .gitignore
├── README.md
└── ...


⚙️ Features
	•	✅ Add/Edit/Delete categories and items
	•	✅ Supplier and purchase order management
	•	✅ Auto-alerts for low stock (via alert_log)
	•	✅ Issue record tracking
	•	✅ MySQL backend integration
	•	✅ Clean UI + responsive layout

🧪 Setup Instructions

1. Clone the repository
git clone https://github.com/your-username/inventory-management.git
cd inventory-management

2. Restore Database
Make sure MySQL is running, then import the schema:
mysql -u root -p < Database/IT_consumables.sql

3. Run the Backend
cd BackendAPI
dotnet build
dotnet run

4. Open Frontend
cd Frontend
npm install 
npm run dev

🔐 Environment Variables

Create an .env or configure appsettings.Development.json:
{
  "ConnectionStrings": 
  {
      "DefaultConnection": "server=localhost;database=IT_consumables;user=root;password=yourpassword"
  }
}
