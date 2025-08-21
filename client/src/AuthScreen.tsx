import React, { useState } from 'react'
import Login from './Login'
import Register from './Register'

export default function AuthScreen() {
  const [showRegister, setShowRegister] = useState(false)
  return showRegister
    ? <Register onSwitch={() => setShowRegister(false)} />
    : <Login onSwitch={() => setShowRegister(true)} />
}
