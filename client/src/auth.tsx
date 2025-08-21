import React, { createContext, useContext, useEffect, useState } from 'react'

type AuthCtx = { token: string | null; email: string | null; login: (t: string, e: string) => void; logout: () => void }
const Ctx = createContext<AuthCtx>({ token: null, email: null, login: () => {}, logout: () => {} })

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<string | null>(null)
  const [email, setEmail] = useState<string | null>(null)
  useEffect(() => {
    setToken(localStorage.getItem('ts_token'))
    setEmail(localStorage.getItem('ts_email'))
  }, [])
  function login(t: string, e: string) { localStorage.setItem('ts_token', t); localStorage.setItem('ts_email', e); setToken(t); setEmail(e) }
  function logout() { localStorage.removeItem('ts_token'); localStorage.removeItem('ts_email'); setToken(null); setEmail(null) }
  return <Ctx.Provider value={{ token, email, login, logout }}>{children}</Ctx.Provider>
}
export function useAuth() { return useContext(Ctx) }
