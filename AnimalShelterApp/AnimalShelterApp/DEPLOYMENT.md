# Animal Shelter App - Deployment Guide

## Quick Deployment to Azure Static Web Apps (FREE)

### Prerequisites
1. Azure account (you already have this!)
2. Azure CLI installed
3. Your code pushed to GitHub

### Step 1: Login to Azure
```bash
az login
```

### Step 2: Create the Static Web App
```bash
# Replace YOUR-GITHUB-USERNAME with your actual GitHub username
az staticwebapp create \
  --name "animalshelterapp-portfolio" \
  --resource-group "rg-portfolio" \
  --source "https://github.com/YOUR-GITHUB-USERNAME/AnimalShelterApp" \
  --location "East US" \
  --branch "main" \
  --app-location "/" \
  --output-location "wwwroot" \
  --login-with-github
```

### Step 3: Push Your Code
```bash
git add .
git commit -m "Deploy to Azure Static Web Apps"
git push origin main
```

### What You Get
✅ **FREE hosting** (no cost added to your $100/month)
✅ **Custom domain** support (yourapp.azurestaticapps.net)
✅ **Automatic HTTPS** 
✅ **Global CDN** for fast loading worldwide
✅ **Automatic deployments** on every push to main branch

### Alternative: Manual Azure Portal Method

If CLI doesn't work:

1. Go to Azure Portal → Create Resource
2. Search "Static Web Apps"
3. Click Create
4. Fill in:
   - **Subscription**: Your subscription
   - **Resource Group**: Create new "rg-portfolio"
   - **Name**: "animalshelterapp-portfolio"
   - **Region**: East US
   - **Deployment Source**: GitHub
   - **GitHub Account**: Link your account
   - **Repository**: Select AnimalShelterApp
   - **Branch**: main
   - **Build Location**: /
   - **App Location**: /
   - **Output Location**: wwwroot

5. Click "Review + Create"

### Your Final URL will be:
`https://animalshelterapp-portfolio.azurestaticapps.net`

## Cost Analysis
- **Azure Static Web Apps**: $0/month (Free tier)
- **Firebase**: $0/month (Free Spark plan for small usage)
- **Total Additional Cost**: $0/month

Your $100/month Azure bill will NOT increase!

## Post-Deployment Steps
1. Test the live site
2. (Optional) Configure custom domain
3. Share the URL in your portfolio
4. Update your resume with the live project link

## Troubleshooting
If build fails:
1. Check GitHub Actions tab in your repository
2. Ensure .NET 9 is specified in workflow
3. Verify Firebase config is not exposing secrets
4. Check Azure portal logs

## Alternative Free Options (if you prefer)
1. **Netlify** - Free static hosting with drag-and-drop
2. **Vercel** - Free with GitHub integration
3. **GitHub Pages** - Free but requires different setup for Blazor
4. **Firebase Hosting** - Free tier available