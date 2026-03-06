import React, { ReactNode } from 'react';
import { Info, AlertTriangle, AlertCircle } from 'lucide-react';

// Define the allowed alert types
type AlertType = 'info' | 'warn' | 'error';

// Define the props for the Alert component
interface AlertProps {
  type?: AlertType;
  title?: string;
  className?: string;
  children: ReactNode;
}

// Define the structure of our style configuration
interface StyleConfigProps {
  container: string;
  icon: ReactNode;
  titleClass: string;
  contentClass: string;
}

const Alert = ({ type = 'info', title, className = '', children }: AlertProps) => {
  // Strongly type the style configuration object
  const styleConfig: Record<AlertType, StyleConfigProps> = {
    info: {
      container: "border-l-4 border-gray-400 bg-gray-50 ring-1 ring-gray-200",
      icon: <Info className="w-5 h-5 shrink-0 text-gray-500" />,
      titleClass: "text-gray-900",
      contentClass: "text-gray-500",
    },
    warn: {
      container: "border-l-4 border-gray-900 bg-white ring-1 ring-gray-200",
      icon: <AlertCircle className="w-5 h-5 shrink-0 text-gray-900" />,
      titleClass: "text-gray-900",
      contentClass: "text-gray-600",
    },
    error: {
      container: "bg-gray-900 ring-1 ring-gray-900",
      icon: <AlertTriangle className="w-5 h-5 shrink-0 text-white" />,
      titleClass: "text-white",
      contentClass: "text-gray-300",
    }
  };

  // Fallback to 'info' if an unknown type is passed
  const currentStyle = styleConfig[type] || styleConfig.info;

  return (
    <div className={`flex items-start gap-3 rounded-lg p-4 shadow-sm transition-all ${currentStyle.container} ${className}`.trim()}>
      {currentStyle.icon}
      <div className="flex-1">
        {title && (
          <h3 className={`text-sm font-medium ${currentStyle.titleClass}`}>
            {title}
          </h3>
        )}
        <div className={`text-sm mt-1 ${currentStyle.contentClass}`}>
          {children}
        </div>
      </div>
    </div>
  );
};

Alert.displayName = "Alert"

export { Alert, type AlertType, type AlertProps }
