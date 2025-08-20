import React, { useState } from 'react'
import TimesheetEditor from './components/TimesheetEditor'
import TimesheetList from './components/TimesheetList'
import TimesheetView from './components/TimesheetView'
import { deleteTimesheet } from './api'

export default function App() {
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [refreshNonce, setRefreshNonce] = useState(0) // force TimesheetList to reload

  async function onDelete() {
    if (!selectedId) return
    if (!confirm('Delete this timesheet? This cannot be undone.')) return
    try {
      await deleteTimesheet(selectedId)
      setSelectedId(null)
      setRefreshNonce(n => n + 1)
    } catch (e: any) {
      alert(e?.message ?? String(e))
    }
  }

  return (
    <div style={{ fontFamily: 'system-ui, sans-serif', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, padding: 24 }}>
      <section>
        <h1 style={{ fontSize: 28, marginBottom: 8 }}>Timesheet Editor</h1>
        <p style={{ color: '#555', marginTop: 0 }}>Add line items (date + minutes), set an hourly rate, and save.</p>
        <TimesheetEditor onSaved={(id) => {
          setSelectedId(id)
          setRefreshNonce(n => n + 1)
        }} />
      </section>
      <section>
        <h2 style={{ fontSize: 22, margin: 0 }}>Your Timesheets</h2>
        {/* key={refreshNonce} forces the list to refetch after deletes/saves */}
        <TimesheetList onSelect={setSelectedId} selectedId={selectedId} key={refreshNonce} />
        {selectedId && (
          <div style={{ marginTop: 16 }}>
            <div style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
              <button
                onClick={onDelete}
                style={{ border: '1px solid #f33', color: '#f33', background: 'transparent', padding: '6px 10px', borderRadius: 8 }}
              >
                Delete
              </button>
            </div>
            <TimesheetView id={selectedId} />
          </div>
        )}
      </section>
    </div>
  )
}
