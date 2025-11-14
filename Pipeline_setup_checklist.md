# Quick Setup Checklist for GitHub Actions Release Pipeline

## ✅ Setup Checklist (5 minutes)

### Step 1: Create Workflow Directory
```bash
mkdir -p .github/workflows
```

### Step 2: Add Workflow File
Copy `release.yml` to `.github/workflows/release.yml`

### Step 3: Configure GitHub Repository
1. Go to GitHub Settings → Actions → General
2. Enable "Read and write permissions"
3. Click Save

### Step 4: Commit and Push
```bash
git add .github/workflows/release.yml
git commit -m "Add automated release pipeline"
git push origin main
```

### Step 5: Verify It Works
Go to Actions tab on GitHub - should see "Build and Release" workflow

## ✅ First Release Checklist

### Before Tagging
- [ ] All changes committed to main
- [ ] CHANGELOG.md updated with new version
- [ ] Code builds locally: `dotnet build --configuration Release`
- [ ] Tests pass locally: `dotnet test`

### Create Release
```bash
# Pull latest
git checkout main
git pull origin main

# Create tag
git tag -a v1.0.0 -m "Initial release"

# Push tag
git push origin v1.0.0
```

### After Tagging
- [ ] Go to Actions tab and watch pipeline run (~10 min)
- [ ] Check Releases tab for new release
- [ ] Download and test one of the binaries
- [ ] Share release link with users! 🎉

## 🎯 Quick Commands Reference

### Create a New Release
```bash
# Update version info
vim CHANGELOG.md

# Commit changes
git add .
git commit -m "Prepare v1.2.0 release"
git push origin main

# Tag and release
git tag -a v1.2.0 -m "Release v1.2.0"
git push origin v1.2.0
```

### Check Pipeline Status
```bash
# Open in browser
open https://github.com/YOUR_USERNAME/CoffeeManagementSoftware/actions

# Or use GitHub CLI
gh run list
gh run watch
```

### Download Latest Release
```bash
# Using GitHub CLI
gh release download

# Or manually from:
# https://github.com/YOUR_USERNAME/CoffeeManagementSoftware/releases/latest
```

## 🚨 Troubleshooting

**Pipeline not running?**
→ Check Actions is enabled in Settings

**Release not created?**
→ Make sure tag starts with 'v' (v1.0.0)

**Build fails?**
→ Test locally first with `dotnet build --configuration Release`

**Permission denied?**
→ Enable write permissions in Settings → Actions

## 📚 Full Documentation

For complete details, see [RELEASE_PIPELINE_GUIDE.md](./RELEASE_PIPELINE_GUIDE.md)