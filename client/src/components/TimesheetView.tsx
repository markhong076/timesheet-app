import React, { useEffect, useState } from 'react'
import { getTimesheet, TimesheetResponse } from '../api'

export default function TimesheetView({ id }: { id: string }) {
  const [ts, setTs] = useState<TimesheetResponse | null>(null)
  const [err, setErr] = useState<string | null>(null)

  useEffect(() => {
    getTimesheet(id).then(setTs).catch(e => setErr(e.message ?? String(e)))
  }, [id])

  if (err) return <div style={{ color: 'crimson' }}>{err}</div>
  if (!ts) return <div>Loading...</div>

  return (
    <div style={{ border: '1px solid #ddd', borderRadius: 12, padding: 12 }}>
      <h3 style={{ marginTop: 0 }}>{ts.description ?? '(No description)'}</h3>
      <div style={{ display: 'flex', gap: 24 }}>
        <div><b>Rate:</b> ${ts.rate.toFixed(2)}/hr</div>
        <div><b>Total:</b> {ts.totalMinutes} min ({ts.totalHours.toFixed(2)} hrs)</div>
        <div><b>Cost:</b> ${ts.totalCost.toFixed(2)}</div>
      </div>

      <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: 12 }}>
        <thead>
          <tr style={{ textAlign: 'left' }}>
            <th style={{ borderBottom: '1px solid #eee', padding: 8 }}>Date</th>
            <th style={{ borderBottom: '1px solid #eee', padding: 8 }}>Minutes</th>
            <th style={{ borderBottom: '1px solid #eee', padding: 8 }}>Notes</th>
          </tr>
        </thead>
        <tbody>
          {ts.lineItems.map(li => (
            <tr key={li.id}>
              <td style={{ padding: 8 }}>{li.date}</td>
              <td style={{ padding: 8 }}>{li.minutes}</td>
              <td style={{ padding: 8 }}>{li.notes ?? ''}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
