let tokenGetter: () => string | null = () => localStorage.getItem('ts_token')
export function setTokenGetter(fn: () => string | null) { tokenGetter = fn }

const base = import.meta.env.VITE_API_BASE ?? "http://localhost:5000";

function authHeaders(): Record<string, string>{
  const t = tokenGetter()
  return t ? { Authorization: `Bearer ${t}` } : {}
}

export async function register(email: string, password: string): Promise<{ token: string; email: string }> {
  const res = await fetch(`${base}/api/auth/register`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  })
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}

export async function login(email: string, password: string): Promise<{ token: string; email: string }> {
  const res = await fetch(`${base}/api/auth/login`, {
    method: 'POST', headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  })
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}
export interface LineItemDto { date: string; minutes: number; notes?: string | null }
export interface CreateTimesheetRequest {
  description?: string | null
  rate: number
  lineItems: LineItemDto[]
}

export interface TimesheetResponse {
  id: string
  description?: string | null
  rate: number
  totalMinutes: number
  totalHours: number
  totalCost: number
  lineItems: { id: string; date: string; minutes: number; notes?: string | null }[]
  createdAt: string
  updatedAt: string
}

export async function listTimesheets(): Promise<TimesheetResponse[]> {
  const res = await fetch(`${base}/api/timesheets`, { headers: { ...authHeaders() } })
  if (!res.ok) throw new Error(await res.text()); return res.json()
}
export async function getTimesheet(id: string): Promise<TimesheetResponse> {
  const res = await fetch(`${base}/api/timesheets/${id}`, { headers: { ...authHeaders() } })
  if (!res.ok) throw new Error(await res.text()); return res.json()
}
export async function createTimesheet(req: CreateTimesheetRequest): Promise<TimesheetResponse> {
  const res = await fetch(`${base}/api/timesheets`, { method: "POST", headers: { "Content-Type": "application/json", ...authHeaders() }, body: JSON.stringify(req) })
  if (!res.ok) throw new Error(await res.text()); return res.json()
}
export async function updateTimesheet(id: string, req: CreateTimesheetRequest): Promise<TimesheetResponse> {
  const res = await fetch(`${base}/api/timesheets/${id}`, { method: "PUT", headers: { "Content-Type": "application/json", ...authHeaders() }, body: JSON.stringify(req) })
  if (!res.ok) throw new Error(await res.text()); return res.json()
}
export async function deleteTimesheet(id: string): Promise<void> {
  const res = await fetch(`${base}/api/timesheets/${id}`, { method: "DELETE", headers: { ...authHeaders() } })
  if (!res.ok) throw new Error(await res.text())
}