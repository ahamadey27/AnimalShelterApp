# Animal Shelter App

A modern web application for managing animal shelter operations, built with Blazor WebAssembly and Firebase.

## 🚀 Live Demo

**Admin Access:**
- **Email:** hamadey@gmail.com
- **Password:** Password123

Use these credentials to explore the full functionality of the application.

## 📋 Features

- **Animal Management**: Add, edit, and track animals in the shelter
- **Medication Scheduling**: Schedule and track medication doses
- **Dashboard**: Real-time medication tracking and administration
- **Medical Reports**: Generate comprehensive medical reports
- **Responsive Design**: Works perfectly on mobile and desktop

## 🛠️ Technology Stack

- **Frontend**: Blazor WebAssembly (.NET 9)
- **Backend**: Google Firebase (Firestore)
- **Authentication**: Firebase Auth
- **Hosting**: Azure Static Web Apps

## 🌐 Deployment

This application is deployed using Azure Static Web Apps, providing:
- ✅ **Free hosting** for static sites
- ✅ Custom domain support
- ✅ Automatic HTTPS
- ✅ Global CDN for fast loading
- ✅ CI/CD via GitHub Actions

### Deployment Steps

1. **Prerequisites**
   - Azure account
   - GitHub repository
   - Azure CLI installed

2. **Create Azure Static Web App**
   ```bash
   # Login to Azure
   az login
   
   # Create resource group (if needed)
   az group create --name rg-animalshelterapp --location eastus
   
   # Create Static Web App
   az staticwebapp create \
     --name animalshelterapp \
     --resource-group rg-animalshelterapp \
     --source https://github.com/YOUR-USERNAME/AnimalShelterApp \
     --location eastus \
     --branch main \
     --app-location "/" \
     --output-location "wwwroot"
   ```

3. **Configure GitHub Secrets**
   - GitHub will automatically create the workflow
   - The deployment token will be added as a secret

4. **Push to Deploy**
   ```bash
   git add .
   git commit -m "Deploy to Azure Static Web Apps"
   git push origin main
   ```

## 🔧 Development

### Local Setup
```bash
# Clone the repository
git clone https://github.com/YOUR-USERNAME/AnimalShelterApp.git
cd AnimalShelterApp

# Restore dependencies
dotnet restore

# Run the application
dotnet run
```

### Project Structure
```
├── Pages/              # Blazor pages/components
├── Services/           # Business logic services
├── Shared/             # Shared models and components
├── Layout/             # Layout components
├── wwwroot/            # Static files
├── infra/              # Azure Bicep templates
└── .github/workflows/  # CI/CD pipeline
```

## 📊 Firebase Configuration

The app uses Firebase for:
- **Firestore**: Document database for animal and medication data
- **Authentication**: User management and security
- **Security Rules**: Data access control

## 🎯 Future Enhancements

- Mobile app (Xamarin/MAUI)
- Advanced reporting and analytics
- Integration with veterinary systems
- Multi-shelter support
- Automated medication reminders

## 📄 License

This project is for portfolio demonstration purposes.

---

Built with ❤️ for animal shelters everywhere.