import React, { useState, useRef, useEffect } from 'react';
import './styles.css';

interface Option {
  value: string;
  label: string;
}

interface GlassSelectProps {
  value: string | number;
  onChange: (e: { target: { value: string } }) => void;
  className?: string;
  children: React.ReactNode;
}

const GlassSelect: React.FC<GlassSelectProps> = ({ value, onChange, className, children }) => {
  const [open, setOpen] = useState(false);
  const ref = useRef<HTMLDivElement>(null);

  const options: Option[] = React.Children.toArray(children)
    .filter((child): child is React.ReactElement<{ value: string | number; children: React.ReactNode }> =>
      React.isValidElement(child) && child.type === 'option'
    )
    .map(child => ({
      value: String(child.props.value),
      label: String(child.props.children),
    }));

  const selectedLabel = options.find(o => o.value === String(value))?.label ?? String(value);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSelect = (optionValue: string) => {
    onChange({ target: { value: optionValue } });
    setOpen(false);
  };

  return (
    <div className={`glass-select-wrapper ${className ?? ''}`} ref={ref}>
      <div className="glass-select-trigger" onClick={() => setOpen(o => !o)}>
        <span>{selectedLabel}</span>
        <svg
          className={`glass-select-arrow ${open ? 'open' : ''}`}
          xmlns="http://www.w3.org/2000/svg"
          width="10"
          height="6"
          viewBox="0 0 10 6"
        >
          <path d="M0 0l5 6 5-6z" fill="currentColor" opacity="0.5" />
        </svg>
      </div>
      {open && (
        <div className="glass-select-dropdown">
          {options.map(option => (
            <div
              key={option.value}
              className={`glass-select-option ${option.value === String(value) ? 'selected' : ''}`}
              onClick={() => handleSelect(option.value)}
            >
              {option.label}
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default GlassSelect;
