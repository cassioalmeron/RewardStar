# Reward Star - HTML/CSS Prototypes

This folder contains static HTML/CSS prototypes reverse-engineered from the Reward Star React application using **Tailwind CSS via CDN** and centralized shared styles.

## Overview

These prototypes were created by analyzing the React components and their CSS styles from the Frontend application, then converting them into standalone HTML files using Tailwind CSS utility classes. Shared styles are centralized in `styles.css` to avoid repetition, following modern prototyping best practices.

## Files

- **index.html** - Landing page with links to all prototypes
- **home.html** - Home page prototype ("The Ultimate Life Game")
- **game.html** - Games Management page with interactive task grid (dark theme)
- **activity.html** - Activity Schedule page with editable table
- **parameters.html** - Parameters/Settings page with API URL configuration
- **styles.css** - Shared CSS variables and component styles (no repetition!)

## Technology Stack

### Tailwind CSS via CDN
- No build process required
- Utility-first CSS framework
- Responsive design with mobile-first approach
- Custom color configuration matching original app

### Shared Styles Architecture
- **styles.css** contains:
  - CSS custom properties (variables) for colors and themes
  - Reusable component classes (level badges, game levels)
  - Utility classes (transitions, scrollbars)
- **HTML files** use:
  - Tailwind utility classes for layout and styling
  - Shared component classes from styles.css
  - Minimal page-specific inline styles only when needed

## How to Use

1. Open `index.html` in your web browser to see the landing page
2. Click on any card to navigate to the specific prototype page
3. Use the navigation menu on each page to switch between different views
4. **No build step required** - just open the HTML files directly in a browser!

## Features Preserved

### Navigation
- Blue navigation bar (#2196F3) with links to all pages
- Hover effects with semi-transparent overlays
- Active state highlighting (bg-white/20)

### Home Page
- Centered welcome message
- Clean, minimalist design
- White content card on gray background

### Game Page
- **Dark theme** (#1a1a1a background)
- Color-coded difficulty levels:
  - Easy: Green (#66bb6a)
  - Medium: Blue (#42a5f5)
  - Hard: Orange (#ffa726)
- Interactive button states with hover animations
- "DONE!" (yellow badge) and "X" (gray) status indicators
- Reset functionality button
- Footer row showing column completion

### Activity Page
- **Light theme** with responsive data table
- Color-coded difficulty levels:
  - Easy: Green (#5cc073)
  - Medium: Blue (#00c1ff)
  - Hard: Orange (#fab95e)
- Editable fields (position, description, level)
- Checkbox controls for weekdays (Monday-Friday)
- Add/Save/Delete action buttons
- Hover effects on table rows (blue highlight)
- Dark input backgrounds for contrast

### Parameters Page
- Simple, clean form layout
- API URL input field with validation styling
- Save button with hover states
- Centered, max-width container

## Design Principles

### No Style Repetition
- **All shared styles** are in `styles.css`
- **Tailwind utilities** handle most styling needs
- **Page-specific styles** are minimal and inline only when truly unique

### Responsive Design
- Mobile-first approach using Tailwind breakpoints
- Grid layouts adapt to screen sizes
- Horizontal scrolling for tables on mobile
- Navigation works on all screen sizes

### Accessibility
- Semantic HTML5 elements
- Proper form labels and input associations
- Keyboard-accessible interactive elements
- Sufficient color contrast ratios

## Technical Details

### Color Variables in styles.css
```css
--color-primary: #2196F3;
--color-success: #4CAF50;
--color-danger: #f44336;
--level-easy: #5cc073;
--level-medium: #00c1ff;
--level-hard: #fab95e;
```

### Shared Component Classes
- `.level-badge-easy/medium/hard` - Activity difficulty badges
- `.game-level-easy/medium/hard` - Game difficulty cells
- `.input-dark` - Dark background inputs
- `.transition-smooth` - Consistent transition animations
- `.custom-scrollbar` - Styled scrollbars

### Tailwind Configuration
Each page extends Tailwind's default theme with custom colors:
```javascript
tailwind.config = {
    theme: {
        extend: {
            colors: {
                'primary': '#2196F3',
                'primary-dark': '#1976D2',
            }
        }
    }
}
```

## Differences from Original

These prototypes are static and do not include:
- Dynamic data loading from APIs (Axios calls)
- State management and React hooks
- Form submissions and validation logic
- Toast notifications (React Toastify)
- localStorage persistence
- Drag and drop functionality (@hello-pangea/dnd)
- Client-side routing (React Router)

## Original React Tech Stack

The source application uses:
- **React** 19.0.0 - UI framework
- **TypeScript** - Type safety
- **Vite** - Build tool
- **React Router DOM** - Client-side routing
- **Axios** - HTTP client
- **React Toastify** - Notifications
- **@hello-pangea/dnd** - Drag and drop

## Prototype Tech Stack

These prototypes use:
- **HTML5** - Semantic markup
- **Tailwind CSS** - Via CDN, no build step
- **CSS3** - Shared styles in styles.css
- **Vanilla JavaScript** - (none currently, but easily extensible)

## Purpose

These prototypes serve as:
- **Visual reference** for UI design and layout
- **Mockups** for stakeholder review and feedback
- **Starting point** for frontend development
- **Documentation** of the application's user interface
- **Standalone demos** that work without build tools or dependencies
- **Design system** showing consistent component patterns

## Benefits of This Approach

1. **Zero Build Setup** - Just open HTML files in a browser
2. **No Style Duplication** - Shared styles in one CSS file
3. **Fast Iterations** - Edit HTML/CSS and refresh browser
4. **Easy Sharing** - Send files or host on any static server
5. **Modern Styling** - Tailwind CSS utility classes
6. **Maintainable** - Changes to shared styles update all pages
7. **Portable** - Works on any device with a browser

## Browser Compatibility

Works in all modern browsers:
- Chrome/Edge (latest)
- Firefox (latest)
- Safari (latest)
- Mobile browsers (iOS Safari, Chrome Mobile)

## Future Enhancements

Potential additions (without breaking the simplicity):
- Vanilla JavaScript for form validation
- Modal dialogs with JavaScript
- Dark mode toggle
- Print stylesheets
- Accessibility enhancements (ARIA labels)
