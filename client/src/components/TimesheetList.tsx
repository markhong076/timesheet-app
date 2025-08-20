import React, { useEffect, useState } from 'react'
import { listTimesheets, TimesheetResponse } from '../api'

export default function TimesheetList({ onSelect, selectedId }: { onSelect: (id: string) => void, selectedId: string | null }) {
  const [data, setData] = useState<TimesheetResponse[]>([])
  const [err, setErr] = useState<string | null>(null)
  useEffect(() => {
    listTimesheets().then(setData).catch(e => setErr(e.message ?? String(e)))
  }, [])

  if (err) return <div style={{ color: 'crimson' }}>{err}</div>
  return (
    <div style={{ border: '1px solid #eee', borderRadius: 12, marginTop: 8 }}>
      {data.map(ts => (
        <div key={ts.id} onClick={() => onSelect(ts.id)} style={{
          padding: 12, borderBottom: '1px solid #f0f0f0', cursor: 'pointer',
          background: selectedId === ts.id ? '#f7f7f7' : 'white'
        }}>
          <div style={{ fontWeight: 600, fontSize: 16 }}>{ts.description ?? '(No description)'}</div>
          <div style={{ fontSize: 12, color: '#666' }}>
            {new Date(ts.createdAt).toLocaleString()} • {ts.totalMinutes} min • ${ts.totalCost.toFixed(2)}
          </div>
        </div>
      ))}
      {data.length === 0 && <div style={{ padding: 12, color: '#777' }}>No timesheets yet.</div>}
    </div>
  )
}
