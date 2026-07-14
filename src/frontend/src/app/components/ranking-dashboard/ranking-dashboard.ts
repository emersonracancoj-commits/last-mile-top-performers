import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal, effect } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { interval, startWith, switchMap, take } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RepartidorRendimientoDto, RepartidoresService } from '../../services/repartidores.service';
import { NotificationService } from '../../services/notification.service';
import { IncidenciaModal } from '../incidencia-modal/incidencia-modal';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-ranking-dashboard',
  imports: [DatePipe, DecimalPipe, IncidenciaModal],
  templateUrl: './ranking-dashboard.html',
  styleUrl: './ranking-dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RankingDashboard {
  private readonly repartidoresService = inject(RepartidoresService);
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  protected readonly repartidores = signal<RepartidorRendimientoDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');
  protected readonly repartidorSeleccionadoId = signal<string | null>(null);

  protected readonly podio = computed(() => this.repartidores().slice(0, 3));
  protected readonly restantes = computed(() => this.repartidores().slice(3));

  constructor() {
    // Polling cada 4 segundos
    interval(4000)
      .pipe(
        startWith(0),
        switchMap(() => this.repartidoresService.getRankingActual()),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: (ranking) => {
          this.repartidores.set(ranking);
          this.loading.set(false);
          this.errorMessage.set('');
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('No se pudo cargar el ranking en tiempo real.');
        },
      });

    // Escuchar eventos en tiempo real via SignalR
    effect(() => {
      const event = this.notificationService.lastEvent();
      if (event && event.eventName === 'meta_ajustada') {
        console.log('Meta ajustada event received, refreshing ranking...');
        this.refrescarRankingAhora();
      }
    });
  }

  protected progreso(item: RepartidorRendimientoDto): number {
    if (item.meta_diaria_actual <= 0) {
      return 0;
    }

    const percentage = (item.entregas_completadas / item.meta_diaria_actual) * 100;
    return Math.min(100, Math.max(0, percentage));
  }

  protected abrirIncidenciaModal(repartidorId: string): void {
    this.repartidorSeleccionadoId.set(repartidorId);
  }

  protected cerrarIncidenciaModal(): void {
    this.repartidorSeleccionadoId.set(null);
  }

  protected refrescarRankingAhora(): void {
    this.repartidoresService
      .getRankingActual()
      .pipe(take(1), takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (ranking) => {
          this.repartidores.set(ranking);
          this.errorMessage.set('');
        },
        error: () => {
          this.errorMessage.set('No se pudo refrescar el ranking al finalizar la incidencia.');
        },
      });
  }

  protected logout(): void {
    this.authService.logout();
  }
}
