import React, { useState, useEffect } from 'react'
import { toast } from 'react-toastify'
import './Styles.css'

const Parameters = () => {
  const [apiUrl, setApiUrl] = useState('')

  // Load API URL from localStorage on component mount
  useEffect(() => {
    const savedApiUrl = localStorage.getItem('apiUrl')
    if (savedApiUrl) {
      setApiUrl(savedApiUrl)
    }
  }, [])

  // Handle input change
  const handleApiUrlChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setApiUrl(event.target.value)
  }

  // Save API URL to localStorage when button is clicked
  const handleSave = () => {
    try {
      localStorage.setItem('apiUrl', apiUrl)
      toast.success('API URL saved successfully!', {
        position: "top-right",
        autoClose: 3000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
      })
    } catch (error) {
      toast.error('Failed to save API URL. Please try again.', {
        position: "top-right",
        autoClose: 5000,
        hideProgressBar: false,
        closeOnClick: true,
        pauseOnHover: true,
        draggable: true,
      })
      console.error('Error saving API URL:', error)
    }
  }

  return (
    <div className="parameters-container">
      <h1 className="parameters-title">Parameters</h1>
      
      <div className="parameters-form-group">
        <label htmlFor="apiUrl" className="parameters-label">
          API URL:
        </label>
        <input
          id="apiUrl"
          type="url"
          value={apiUrl}
          onChange={handleApiUrlChange}
          placeholder="https://api.example.com"
          className="parameters-input"
        />
      </div>

      <button 
        onClick={handleSave}
        className="parameters-save-button"
        disabled={!apiUrl.trim()}
      >
        Save
      </button>
    </div>
  )
}

export default Parameters