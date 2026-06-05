self.addEventListener('push', (event) => {
  const payload = event.data?.json?.() ?? {}
  const title = payload.title ?? 'fieldcompanion'
  const body = payload.body ?? 'You have a new field notification.'

  const show = self.registration.showNotification(title, {
    body,
    data: payload.data ?? {},
    tag: payload.data?.notificationId ?? 'fieldcompanion-notification',
  })

  const notifyClients = self.clients
    .matchAll({ type: 'window', includeUncontrolled: true })
    .then((clients) => {
      for (const client of clients) {
        client.postMessage({ type: 'fieldcompanion-push', payload })
      }
    })

  event.waitUntil(Promise.all([show, notifyClients]))
})

self.addEventListener('notificationclick', (event) => {
  event.notification.close()
  event.waitUntil(
    self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
      if (clientList.length > 0) {
        return clientList[0].focus()
      }
      return self.clients.openWindow('/')
    }),
  )
})
