import React from 'react'
import { useAuth } from './auth'
import AuthScreen from './AuthScreen'
import AuthedApp from './AuthedApp'

export default function App() {
  const { token } = useAuth() // always called, order never changes
  return token ? <AuthedApp /> : <AuthScreen />
}
