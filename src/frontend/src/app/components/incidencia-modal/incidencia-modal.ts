import { ChangeDetectionStrategy, Component, EventEmitter, inject, input, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RepartidoresService } from '../../services/repartidores.service';

@Component({
  selector: 'app-incidencia-modal',
  imports: [FormsModule],
  templateUrl: './incidencia-modal.html',
  styleUrl: './incidencia-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IncidenciaModal {
  private readonly repartidoresService = inject(RepartidoresService);

  readonly repartidorId = input.required<string>();

  @Output() readonly closed = new EventEmitter<void>();
  @Output() readonly approved = new EventEmitter<void>();

  protected descripcion = '';
  protected nuevaMeta = 10;
  protected readonly loading = signal(false);
  protected readonly errorMessage = signal('');

  protected cancelar(): void {
    if (this.loading()) {
      return;
    }

    this.closed.emit();
  }

  protected aprobar(): void {
    if (this.loading()) {
      return;
    }

    if (!this.descripcion.trim()) {
      this.errorMessage.set('La descripcion es requerida.');
      return;
    }

    if (this.nuevaMeta <= 0) {
      this.errorMessage.set('La nueva meta debe ser mayor que cero.');
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.repartidoresService
      .ajustarMeta(this.repartidorId(), this.descripcion.trim(), this.nuevaMeta)
      .subscribe({
        next: () => {
          this.loading.set(false);
          this.approved.emit();
          this.closed.emit();
        },
        error: () => {
          this.loading.set(false);
          this.errorMessage.set('No fue posible aprobar la incidencia.');
        },
      });
  }
}
