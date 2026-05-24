import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="modal-overlay" *ngIf="isOpen" (click)="onOverlayClick($event)">
      <div class="modal-container" [style.maxWidth]="maxWidth" role="dialog" [attr.aria-label]="title">
        <div class="modal-header">
          <h2 class="modal-title">{{ title }}</h2>
          <button class="modal-close" (click)="close.emit()" aria-label="Close modal">
            <span class="material-symbols-outlined">close</span>
          </button>
        </div>
        <div class="modal-body">
          <ng-content />
        </div>
        <div class="modal-footer" *ngIf="showFooter">
          <ng-content select="[modal-footer]" />
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      inset: 0;
      background: rgba(25, 28, 30, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      padding: 24px;
      animation: fadeOverlay 0.2s ease;
    }

    @keyframes fadeOverlay {
      from { opacity: 0; }
      to   { opacity: 1; }
    }

    .modal-container {
      background: var(--color-surface-container-lowest);
      border: 1px solid var(--color-outline-variant);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-xl);
      width: 100%;
      max-height: 90vh;
      overflow: hidden;
      display: flex;
      flex-direction: column;
      animation: slideModal 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
    }

    @keyframes slideModal {
      from { opacity: 0; transform: translateY(-20px) scale(0.97); }
      to   { opacity: 1; transform: translateY(0) scale(1); }
    }

    .modal-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 20px 24px;
      border-bottom: 1px solid var(--color-outline-variant);
      flex-shrink: 0;
    }

    .modal-title {
      font-size: 18px;
      font-weight: 600;
      color: var(--color-on-surface);
    }

    .modal-close {
      width: 32px;
      height: 32px;
      border-radius: var(--radius);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--color-on-surface-variant);
      transition: background 0.15s ease;

      &:hover { background: var(--color-surface-container); }
    }

    .modal-body {
      padding: 24px;
      overflow-y: auto;
      flex: 1;
    }

    .modal-footer {
      padding: 16px 24px;
      border-top: 1px solid var(--color-outline-variant);
      display: flex;
      justify-content: flex-end;
      gap: 8px;
      flex-shrink: 0;
    }
  `]
})
export class ModalComponent {
  @Input() title: string = '';
  @Input() isOpen: boolean = false;
  @Input() maxWidth: string = '560px';
  @Input() showFooter: boolean = true;
  @Output() close = new EventEmitter<void>();

  onOverlayClick(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal-overlay')) {
      this.close.emit();
    }
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    if (this.isOpen) this.close.emit();
  }
}
