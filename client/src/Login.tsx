import React, { useState } from 'react'
import { login } from './api'
import { useAuth } from './auth'

export default function Login({ onSwitch }: { onSwitch: () => void }) {
  const { login: setAuth } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [err, setErr] = useState<string | null>(null)
  const [busy, setBusy] = useState(false)

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setBusy(true); setErr(null)
    try {
      const res = await login(email, password)
      setAuth(res.token, res.email)
    } catch (e: any) { setErr(e.message ?? String(e)) } finally { setBusy(false) }
  }

  return (
    <form onSubmit={onSubmit} style={{ maxWidth: 360, margin: '10vh auto', padding: 24, border: '1px solid #ddd', borderRadius: 12, fontFamily: 'system-ui' }}>
      <h2>Sign in</h2>
      <input placeholder="Email" type="email" value={email} onChange={e => setEmail(e.target.value)} />
      <input placeholder="Password" type="password" value={password} onChange={e => setPassword(e.target.value)} style={{ marginTop: 8 }} />
      <button disabled={busy} type="submit" style={{ marginTop: 12, padding: '8px 12px' }}>{busy ? 'Signing in…' : 'Sign in'}</button>
      {err && <div style={{ color: 'crimson', marginTop: 8 }}>{err}</div>}
      <div style={{ marginTop: 10, fontSize: 13 }}>
        No account? <a href="#" onClick={(e) => { e.preventDefault(); onSwitch(); }}>Sign up</a>
      </div>
    </form>
  )
}
