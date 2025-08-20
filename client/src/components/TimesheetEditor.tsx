import React, { useMemo, useState } from 'react'
import { createTimesheet, LineItemDto } from '../api'

type Props = { onSaved: (id: string) => void }

export default function TimesheetEditor({ onSaved }: Props) {
  const [description, setDescription] = useState('')
  const [rate, setRate] = useState<number>(120)
  const [items, setItems] = useState<LineItemDto[]>([
    { date: new Date().toISOString().slice(0,10), minutes: 60, notes: '' },
  ])
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const totalMinutes = useMemo(() => items.reduce((a, b) => a + (b.minutes || 0), 0), [items])
  const totalHours = useMemo(() => Math.round((totalMinutes / 60) * 100) / 100, [totalMinutes])
  const totalCost = useMemo(() => Math.round((totalHours * (rate || 0)) * 100) / 100, [rate, totalHours])

  function updateItem(idx: number, patch: Partial<LineItemDto>) {
    setItems(prev => prev.map((it, i) => i === idx ? { ...it, ...patch } : it))
  }

  function addItem() {
    setItems(prev => [...prev, { date: new Date().toISOString().slice(0,10), minutes: 0, notes: '' }])
  }

  function removeItem(idx: number) {
    setItems(prev => prev.filter((_, i) => i !== idx))
  }

  async function save() {
    setSaving(true); setError(null)
    try {
      const payload = { description: description || null, rate: rate || 0, lineItems: items }
      const created = await createTimesheet(payload)
      onSaved(created.id)
    } catch (e: any) {
      setError(e.message ?? String(e))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div style={{ border: '1px solid #ddd', borderRadius: 12, padding: 16 }}>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 160px', gap: 12 }}>
        <div>
          <label style={{ fontWeight: 600 }}>Description</label>
          <textarea value={description} onChange={e => setDescription(e.target.value)} rows={3} style={{ width: '100%', padding: 8, borderRadius: 8, border: '1px solid #ccc' }} />
        </div>
        <div>
          <label style={{ fontWeight: 600 }}>Hourly Rate</label>
          <input type="number" value={rate} onChange={e => setRate(parseFloat(e.target.value))} style={{ width: '100%', padding: 8, borderRadius: 8, border: '1px solid #ccc' }} />
          <div style={{ fontSize: 12, color: '#666', marginTop: 4 }}>Assumed per hour</div>
        </div>
      </div>

      <h3 style={{ marginTop: 16 }}>Line Items</h3>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ textAlign: 'left' }}>
            <th style={{ borderBottom: '1px solid #eee', padding: 8 }}>Date</th>
            <th style={{ borderBottom: '1px solid #eee', padding: 8 }}>Minutes</th>
            <th style={{ borderBottom: '1px solid #eee', padding: 8 }}>Notes</th>
            <th style={{ borderBottom: '1px solid #eee', padding: 8 }}></th>
          </tr>
        </thead>
        <tbody>
          {items.map((it, idx) => (
            <tr key={idx}>
              <td style={{ padding: 8 }}>
                <input type="date" value={it.date} onChange={e => updateItem(idx, { date: e.target.value })} />
              </td>
              <td style={{ padding: 8 }}>
                <input type="number" value={it.minutes} onChange={e => updateItem(idx, { minutes: parseInt(e.target.value || '0', 10) })} min={0} />
              </td>
              <td style={{ padding: 8 }}>
                <input type="text" value={it.notes ?? ''} onChange={e => updateItem(idx, { notes: e.target.value })} placeholder="Optional" style={{ width: '100%' }} />
              </td>
              <td style={{ padding: 8 }}>
                <button onClick={() => removeItem(idx)} style={{ background: 'transparent', border: '1px solid #ccc', padding: '4px 8px', borderRadius: 8 }}>Delete</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <button onClick={addItem} style={{ marginTop: 8, border: '1px solid #ccc', padding: '6px 10px', borderRadius: 8 }}>+ Add Line</button>

      <div style={{ display: 'flex', gap: 24, marginTop: 16 }}>
        <div><b>Total Minutes:</b> {totalMinutes}</div>
        <div><b>Total Hours:</b> {totalHours.toFixed(2)}</div>
        <div><b>Total Cost:</b> {totalCost.toFixed(2)}</div>
      </div>

      {error && <div style={{ color: 'crimson', marginTop: 8 }}>{error}</div>}
      <div style={{ marginTop: 12 }}>
        <button onClick={save} disabled={saving} style={{ background: '#111', color: 'white', padding: '8px 14px', borderRadius: 10, border: 'none' }}>
          {saving ? 'Saving...' : 'Save Timesheet'}
        </button>
      </div>
    </div>
  )
}
