import React from 'react'
import ReactDOM from 'react-dom/client'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import App from './App'
import { AuthProvider, useAuth } from './auth'
import { setTokenGetter } from './api'

function TokenBridge() {
  const { token } = useAuth()
  // keep API layer in sync with the current token
  setTokenGetter(() => token)
  return null
}

const router = createBrowserRouter([
  { path: '/', element: <App /> },
])

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AuthProvider>
      <TokenBridge />
      <RouterProvider router={router} />
    </AuthProvider>
  </React.StrictMode>,
)
