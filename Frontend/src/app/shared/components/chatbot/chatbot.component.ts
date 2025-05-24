import { Component, Input } from '@angular/core';
import { HttpClient, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.component.html',
  styleUrl: './chatbot.component.css'
})
export class ChatbotComponent {
  @Input() show = false;
  hasGreeted = false;
  baseUrl = environment.baseUrl + 'chat/send';

  messages: { role: 'user' | 'assistant' | 'system', content: string }[] = [
    { 
      role: 'system', 
      content: 'Actuás como un soporte técnico amable y claro para Cáritas, explicando cómo usar el sistema de gestión de inventario.' 
    }
  ];
  
  userInput: string = '';
  isLoading: boolean = false;

  constructor(private http: HttpClient) {}

  async sendMessage() {
    if (!this.userInput.trim()) return;

    const userMessage = this.userInput.trim();
    this.messages.push({ role: 'user', content: userMessage });
    this.userInput = '';
    this.isLoading = true;

    try {
      // Preparamos los mensajes en el formato que espera el backend
      const messagesToSend = this.messages
        .filter(msg => msg.role !== 'system') // Excluimos el mensaje de sistema
        .map(msg => ({
          Role: msg.role.charAt(0).toUpperCase() + msg.role.slice(1), // "user" -> "User"
          Content: msg.content
        }));

      const response = await this.http.post<{message: string}>(
        this.baseUrl,
        messagesToSend,
        { 
          headers: new HttpHeaders({ 
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          })
        }
      ).toPromise();

      if (response) {
        this.messages.push({ 
          role: 'assistant', 
          content: response.message 
        });
      } else {
        throw new Error('La respuesta del servidor está vacía');
      }
    } catch (error) {
      console.error('Error:', error);
      this.handleError(error);
    } finally {
      this.isLoading = false;
    }
  }

  private handleError(error: any) {
    let errorMessage = 'Hubo un error al contactar con el asistente. Por favor, inténtalo de nuevo más tarde.';
    
    if (error instanceof HttpErrorResponse) {
      // Intentamos extraer el mensaje de error de la respuesta
      try {
        const errorObj = error.error ? (typeof error.error === 'string' ? JSON.parse(error.error) : error.error) : {};
        errorMessage = errorObj.message || errorObj.error || error.message;
      } catch (e) {
        // Si falla el parsing, usamos el texto directo
        errorMessage = typeof error.error === 'string' ? error.error : error.message;
      }
    } else if (error.error instanceof ErrorEvent) {
      // Error del lado del cliente
      errorMessage = `Error: ${error.error.message}`;
    } else {
      // Otro tipo de error
      errorMessage = error.message || error.toString();
    }

    this.messages.push({ 
      role: 'assistant', 
      content: errorMessage 
    });
  }

  toggleChat() {
    this.show = !this.show;

    if (this.show && !this.hasGreeted) {
      this.messages.push({
        role: 'assistant',
        content: '¡Hola! Soy Botín de Cáritas, tu asistente para el sistema de gestión de inventario. ¿En qué puedo ayudarte hoy?'
      });
      this.hasGreeted = true;
    }
  }
}