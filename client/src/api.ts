const base = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000'

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
  const res = await fetch(`${base}/api/timesheets`)
  if (!res.ok) throw new Error('Failed to list timesheets')
  return res.json()
}

export async function getTimesheet(id: string): Promise<TimesheetResponse> {
  const res = await fetch(`${base}/api/timesheets/${id}`)
  if (!res.ok) throw new Error('Not found')
  return res.json()
}

export async function createTimesheet(req: CreateTimesheetRequest): Promise<TimesheetResponse> {
  const res = await fetch(`${base}/api/timesheets`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(req),
  })
  if (!res.ok) throw new Error(await res.text())
  return res.json()
}

export async function updateTimesheet(id: string, req: CreateTimesheetRequest): Promise<TimesheetResponse> {
  const res = await fetch(`${base}/api/timesheets/${id}`, {
    method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify(req)
  });
  if (!res.ok) throw new Error(await res.text());
  return res.json();
}

export async function deleteTimesheet(id: string): Promise<void> {
  const res = await fetch(`${base}/api/timesheets/${id}`, { method: "DELETE" });
  if (!res.ok) throw new Error(await res.text());
}