#!/bin/bash

echo "🚀 Building Blazor PWA for Production (Cloudflare Pages)"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean --configuration Release
rm -rf bin/Release
rm -rf obj/Release

# Restore packages
echo "📦 Restoring packages..."
dotnet restore

# Build for production (without AOT to avoid wasm-tools dependency)
echo "🔨 Building for Release..."
dotnet publish -c Release -o dist --nologo -p:DisableAOT=true

# Verify critical files exist
echo "✅ Verifying build output..."
if [ -f "dist/wwwroot/_framework/blazor.webassembly.js" ]; then
    echo "✅ Blazor WebAssembly runtime found"
else
    echo "❌ Blazor WebAssembly runtime missing!"
    exit 1
fi

if [ -f "dist/wwwroot/_framework/dotnet.wasm" ]; then
    echo "✅ .NET WebAssembly runtime found"
else
    echo "❌ .NET WebAssembly runtime missing!"
    exit 1
fi

# Check for compressed files
echo "📊 Checking compression..."
find dist/wwwroot/_framework -name "*.br" | head -5
find dist/wwwroot/_framework -name "*.gz" | head -5

# Check MinimalLayout is in the build
echo "🔍 Checking MinimalLayout in build..."
if find dist/wwwroot/_framework -name "*.dll" -exec grep -l "MinimalLayout" {} \; | head -1; then
    echo "✅ MinimalLayout found in build"
else
    echo "⚠️  MinimalLayout not found in build - check linker.xml"
fi

echo "🎉 Build completed successfully!"
echo "📁 Output directory: dist/wwwroot"
echo "🌐 Ready for Cloudflare Pages deployment"
echo ""
echo "📋 Deploy Instructions:"
echo "1. Build command: ./build-production.sh"
echo "2. Output directory: dist/wwwroot"
echo "3. Environment variables: none required"