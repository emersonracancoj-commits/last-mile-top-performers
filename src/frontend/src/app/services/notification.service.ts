import { Injectable, signal, Signal, inject } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';
import { AuthService } from '../auth/auth.service';

export interface WebSocketEvent<T = any> {
  eventName: string;
  payload: T;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly authService = inject(AuthService);
  
  private hubConnection: HubConnection | null = null;
  private readonly hubUrl = 'http://localhost:5000/hubs/notification'; // Ajustar según entorno

  // Signal expuesto para que componentes reactivos se suscriban
  private readonly lastEventSignal = signal<WebSocketEvent | null>(null);
  public readonly lastEvent: Signal<WebSocketEvent | null> = this.lastEventSignal.asReadonly();

  // Signal para el estado de conexión
  private readonly isConnectedSignal = signal<boolean>(false);
  public readonly isConnected: Signal<boolean> = this.isConnectedSignal.asReadonly();

  // Signal para errores de conexión
  private readonly connectionErrorSignal = signal<string | null>(null);
  public readonly connectionError: Signal<string | null> = this.connectionErrorSignal.asReadonly();

  async startConnection(): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      console.log('SignalR connection already established');
      return;
    }

    try {
      const token = this.authService.getAccessToken();
      
      this.hubConnection = new HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          withCredentials: true,
          accessTokenFactory: () => token || ''
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .configureLogging(LogLevel.Warning)
        .build();

      // Escuchar evento genérico del backend
      this.hubConnection.on('ReceiveEvent', (eventData: any) => {
        console.log('Event received from backend:', eventData);
        this.lastEventSignal.set({
          eventName: eventData.eventName || 'unknown',
          payload: eventData,
          timestamp: new Date()
        });
      });

      // Escuchar eventos específicos (si el backend los envía)
      this.hubConnection.on('meta_ajustada', (eventData: any) => {
        console.log('Meta ajustada event:', eventData);
        this.lastEventSignal.set({
          eventName: 'meta_ajustada',
          payload: eventData,
          timestamp: new Date()
        });
      });

      this.hubConnection.on('transaccion_recibida', (eventData: any) => {
        console.log('Transacción recibida event:', eventData);
        this.lastEventSignal.set({
          eventName: 'transaccion_recibida',
          payload: eventData,
          timestamp: new Date()
        });
      });

      // Manejo de eventos de conexión
      this.hubConnection.onreconnected(() => {
        console.log('SignalR reconnected');
        this.isConnectedSignal.set(true);
        this.connectionErrorSignal.set(null);
      });

      this.hubConnection.onreconnecting((error?: Error) => {
        console.log('SignalR reconnecting:', error?.message);
        this.isConnectedSignal.set(false);
      });

      this.hubConnection.onclose((error?: Error) => {
        console.log('SignalR connection closed:', error?.message);
        this.isConnectedSignal.set(false);
        this.connectionErrorSignal.set(error?.message || 'Connection closed');
      });

      await this.hubConnection.start();
      this.isConnectedSignal.set(true);
      this.connectionErrorSignal.set(null);
      console.log('SignalR connection started successfully');
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Unknown error';
      console.error('SignalR connection failed:', errorMessage);
      this.isConnectedSignal.set(false);
      this.connectionErrorSignal.set(errorMessage);
      throw error;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        this.isConnectedSignal.set(false);
        console.log('SignalR connection stopped');
      } catch (error) {
        console.error('Error stopping SignalR connection:', error);
      }
    }
  }

  /**
   * Obtiene el token de autenticación desde AuthService
   */
  private getAuthToken(): string {
    return this.authService.getAccessToken() || '';
  }
}
