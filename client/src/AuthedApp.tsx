import React, { useEffect, useState } from 'react'
import { useAuth } from './auth'
import TimesheetEditor from './components/TimesheetEditor'
import TimesheetList from './components/TimesheetList'
import TimesheetView from './components/TimesheetView'
import { deleteTimesheet, getTimesheet, TimesheetResponse } from './api'

export default function AuthedApp() {
  const { email, logout } = useAuth()
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [editId, setEditId] = useState<string | null>(null)
  const [editing, setEditing] = useState<TimesheetResponse | null>(null)
  const [refreshNonce, setRefreshNonce] = useState(0)

  useEffect(() => {
    if (!editId) { setEditing(null); return }
    getTimesheet(editId).then(setEditing).catch(() => setEditing(null))
  }, [editId])

  async function onDelete() {
    if (!selectedId) return
    if (!confirm('Delete this timesheet?')) return
    await deleteTimesheet(selectedId)
    setSelectedId(null)
    setEditId(null)
    setRefreshNonce(n => n + 1)
  }

  return (
    <div style={{ fontFamily: 'system-ui, sans-serif', padding: 24 }}>
      <div style={{ display:'flex', justifyContent:'space-between', alignItems:'center', marginBottom: 12 }}>
        <div>Signed in as <b>{email}</b></div>
        <button onClick={logout} style={{ border:'1px solid #ccc', padding:'6px 10px', borderRadius:8 }}>Logout</button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
        <section>
          <h1 style={{ fontSize: 28, marginBottom: 8 }}>{editing ? 'Edit Timesheet' : 'Timesheet Editor'}</h1>
          <p style={{ color: '#555', marginTop: 0 }}>{editing ? 'Modify and save changes.' : 'Add line items and save.'}</p>
          <TimesheetEditor
            initial={editing}
            onSaved={(id) => {
              setSelectedId(id)
              setEditId(null)
              setRefreshNonce(n => n + 1)
            }}
          />
        </section>

        <section>
          <h2 style={{ fontSize: 22, margin: 0 }}>Your Timesheets</h2>
          <TimesheetList onSelect={setSelectedId} selectedId={selectedId} key={refreshNonce} />
          {selectedId && (
            <div style={{ marginTop: 16 }}>
              <div style={{ display: 'flex', gap: 8, marginBottom: 8 }}>
                <button onClick={() => setEditId(selectedId)} style={{ border:'1px solid #ccc', padding:'6px 10px', borderRadius:8 }}>Edit</button>
                <button onClick={onDelete} style={{ border:'1px solid #f33', color:'#f33', background:'transparent', padding:'6px 10px', borderRadius:8 }}>Delete</button>
              </div>
              <TimesheetView id={selectedId} />
            </div>
          )}
        </section>
      </div>
    </div>
  )
}
