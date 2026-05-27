namespace NtfyDesktop.Features.Connections;

// Pure connection-health enum (no notification concerns).
//   Connected    — all configured topics have an open socket.
//   Degraded     — at least one topic isn't Connected (connecting, flapping, failed).
//   Disconnected — no live sockets at all: either zero topics configured, or the
//                  user explicitly tore everything down ("Disconnect all").
public enum ConnectionStatus
{
    Connected,
    Degraded,
    Disconnected,
}
