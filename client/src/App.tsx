import React, { useState } from 'react'
import TimesheetEditor from './components/TimesheetEditor'
import TimesheetList from './components/TimesheetList'
import TimesheetView from './components/TimesheetView'

export default function App() {
  const [selectedId, setSelectedId] = useState<string | null>(null)
  return (
    <div style={{ fontFamily: 'system-ui, sans-serif', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24, padding: 24 }}>
      <section>
        <h1 style={{ fontSize: 28, marginBottom: 8 }}>Timesheet Editor</h1>
        <p style={{ color: '#555', marginTop: 0 }}>Add line items, set an hourly rate, and save.</p>
        <TimesheetEditor onSaved={(id) => setSelectedId(id)} />
      </section>
      <section>
        <h2 style={{ fontSize: 22, margin: 0 }}>Your Timesheets</h2>
        <TimesheetList onSelect={setSelectedId} selectedId={selectedId} />
        {selectedId && (
          <div style={{ marginTop: 16 }}>
            <TimesheetView id={selectedId} />
          </div>
        )}
      </section>
    </div>
  )
}
