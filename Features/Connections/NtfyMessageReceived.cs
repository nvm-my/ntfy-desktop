using FastEndpoints;
using NtfyDesktop.Domain;

namespace NtfyDesktop.Features.Connections;

public record NtfyMessageReceived(NtfyMessage Message) : IEvent;
