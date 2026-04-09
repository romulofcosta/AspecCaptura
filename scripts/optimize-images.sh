#!/bin/bash

###############################################################################
# Image Optimization Script
# Converts images to WebP format and creates responsive variants
# Requirements: 6.7, 12.5, 12.6
###############################################################################

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
IMAGES_DIR="wwwroot/images"
MAX_WIDTH=1920
QUALITY=85
WEBP_QUALITY=80

echo -e "${GREEN}=== Image Optimization Script ===${NC}"
echo ""

# Check if ImageMagick is installed
if ! command -v convert &> /dev/null; then
    echo -e "${RED}Error: ImageMagick is not installed${NC}"
    echo "Install it with:"
    echo "  Ubuntu/Debian: sudo apt-get install imagemagick"
    echo "  macOS: brew install imagemagick"
    echo "  Windows: choco install imagemagick"
    exit 1
fi

# Check if cwebp is installed (for WebP conversion)
if ! command -v cwebp &> /dev/null; then
    echo -e "${YELLOW}Warning: cwebp is not installed, WebP conversion will be skipped${NC}"
    echo "Install it with:"
    echo "  Ubuntu/Debian: sudo apt-get install webp"
    echo "  macOS: brew install webp"
    echo "  Windows: choco install webp"
    SKIP_WEBP=true
else
    SKIP_WEBP=false
fi

echo ""
echo "Configuration:"
echo "  Images directory: $IMAGES_DIR"
echo "  Max width: ${MAX_WIDTH}px"
echo "  JPEG quality: $QUALITY"
echo "  WebP quality: $WEBP_QUALITY"
echo ""

# Create output directory if it doesn't exist
mkdir -p "$IMAGES_DIR/optimized"

# Counter for statistics
TOTAL_FILES=0
OPTIMIZED_FILES=0
WEBP_FILES=0
TOTAL_ORIGINAL_SIZE=0
TOTAL_OPTIMIZED_SIZE=0

# Process all images
for img in "$IMAGES_DIR"/*.{jpg,jpeg,png,JPG,JPEG,PNG} 2>/dev/null; do
    # Skip if no files found
    [ -e "$img" ] || continue
    
    TOTAL_FILES=$((TOTAL_FILES + 1))
    
    # Get filename without extension
    filename=$(basename "$img")
    name="${filename%.*}"
    ext="${filename##*.}"
    
    echo -e "${YELLOW}Processing: $filename${NC}"
    
    # Get original size
    original_size=$(stat -f%z "$img" 2>/dev/null || stat -c%s "$img" 2>/dev/null)
    TOTAL_ORIGINAL_SIZE=$((TOTAL_ORIGINAL_SIZE + original_size))
    
    # Optimize JPEG/PNG
    if [[ "$ext" =~ ^(jpg|jpeg|JPG|JPEG)$ ]]; then
        # Optimize JPEG
        convert "$img" \
            -resize "${MAX_WIDTH}x>" \
            -quality $QUALITY \
            -strip \
            -interlace Plane \
            "$IMAGES_DIR/optimized/${name}.jpg"
        
        OPTIMIZED_FILES=$((OPTIMIZED_FILES + 1))
        
        # Create responsive variants
        convert "$img" \
            -resize "640x>" \
            -quality $QUALITY \
            -strip \
            "$IMAGES_DIR/optimized/${name}-640w.jpg"
        
        convert "$img" \
            -resize "1280x>" \
            -quality $QUALITY \
            -strip \
            "$IMAGES_DIR/optimized/${name}-1280w.jpg"
        
        echo "  ✓ Created JPEG variants (640w, 1280w, ${MAX_WIDTH}w)"
        
    elif [[ "$ext" =~ ^(png|PNG)$ ]]; then
        # Optimize PNG
        convert "$img" \
            -resize "${MAX_WIDTH}x>" \
            -strip \
            "$IMAGES_DIR/optimized/${name}.png"
        
        OPTIMIZED_FILES=$((OPTIMIZED_FILES + 1))
        
        echo "  ✓ Optimized PNG"
    fi
    
    # Convert to WebP
    if [ "$SKIP_WEBP" = false ]; then
        cwebp -q $WEBP_QUALITY "$img" -o "$IMAGES_DIR/optimized/${name}.webp" > /dev/null 2>&1
        
        # Create responsive WebP variants
        convert "$img" -resize "640x>" /tmp/temp-640.png
        cwebp -q $WEBP_QUALITY /tmp/temp-640.png -o "$IMAGES_DIR/optimized/${name}-640w.webp" > /dev/null 2>&1
        rm /tmp/temp-640.png
        
        convert "$img" -resize "1280x>" /tmp/temp-1280.png
        cwebp -q $WEBP_QUALITY /tmp/temp-1280.png -o "$IMAGES_DIR/optimized/${name}-1280w.webp" > /dev/null 2>&1
        rm /tmp/temp-1280.png
        
        WEBP_FILES=$((WEBP_FILES + 1))
        echo "  ✓ Created WebP variants (640w, 1280w, ${MAX_WIDTH}w)"
    fi
    
    # Get optimized size
    if [ -f "$IMAGES_DIR/optimized/${name}.jpg" ]; then
        optimized_size=$(stat -f%z "$IMAGES_DIR/optimized/${name}.jpg" 2>/dev/null || stat -c%s "$IMAGES_DIR/optimized/${name}.jpg" 2>/dev/null)
    elif [ -f "$IMAGES_DIR/optimized/${name}.png" ]; then
        optimized_size=$(stat -f%z "$IMAGES_DIR/optimized/${name}.png" 2>/dev/null || stat -c%s "$IMAGES_DIR/optimized/${name}.png" 2>/dev/null)
    else
        optimized_size=$original_size
    fi
    
    TOTAL_OPTIMIZED_SIZE=$((TOTAL_OPTIMIZED_SIZE + optimized_size))
    
    # Calculate savings
    savings=$((original_size - optimized_size))
    savings_percent=$((savings * 100 / original_size))
    
    echo "  Size: $(numfmt --to=iec $original_size) → $(numfmt --to=iec $optimized_size) (${savings_percent}% reduction)"
    echo ""
done

# Print statistics
echo -e "${GREEN}=== Optimization Complete ===${NC}"
echo ""
echo "Statistics:"
echo "  Total files processed: $TOTAL_FILES"
echo "  Optimized files: $OPTIMIZED_FILES"
echo "  WebP files created: $WEBP_FILES"
echo ""
echo "  Original total size: $(numfmt --to=iec $TOTAL_ORIGINAL_SIZE)"
echo "  Optimized total size: $(numfmt --to=iec $TOTAL_OPTIMIZED_SIZE)"

if [ $TOTAL_ORIGINAL_SIZE -gt 0 ]; then
    total_savings=$((TOTAL_ORIGINAL_SIZE - TOTAL_OPTIMIZED_SIZE))
    total_savings_percent=$((total_savings * 100 / TOTAL_ORIGINAL_SIZE))
    echo "  Total savings: $(numfmt --to=iec $total_savings) (${total_savings_percent}%)"
fi

echo ""
echo -e "${GREEN}Optimized images are in: $IMAGES_DIR/optimized/${NC}"
echo ""
echo "Next steps:"
echo "  1. Review optimized images"
echo "  2. Replace original images with optimized versions"
echo "  3. Update image references to use WebP with fallback"
echo ""

