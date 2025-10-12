# ClubEngine - Copilot Instructions

## Project Overview

ClubEngine is a comprehensive club management system. It consists of those components:
- Angular Frontend (folder ClubEngine.Web)
- .NET/C# Backend (folder ClubEngine.Api)
- SQL Server Database, the structure is managed by Entity Framework Core Migrations

It provides tools for managing club memberships, member data, fees, and administrative tasks. The application uses a multi-tenant architecture where users can select and manage different clubs (partitions).
It uses delegates cross cutting concerns like authentication, authorization, and notifications to the library AppEngine. AppEngine should be usable by other applications as well.

## Technology Stack

### Frontend
- **Framework**: Angular 19.0.5
- **UI Library**: Angular Material 19.0.4
- **Template**: Fuse Admin Template (Angular version)
- **Styling**: Tailwind CSS with custom configuration
- **Authentication**: Auth0 Angular SDK
- **Internationalization**: Transloco (ngx-translate), but the texts are loaded at runtime from the backend
- **Charts**: ApexCharts with ng-apexcharts
- **Real-time**: Microsoft SignalR

### Backend
- **Framework**: .NET 6
- **Architecture**: Mediator pattern
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: Auth0 .NET SDK
- **API Documentation**: Swagger/OpenAPI
- **Internationalization**: resx files
- **Real-time**: Microsoft SignalR


### Development Tools
- **TypeScript**: Latest version with strict configuration
- **Build Tool**: Angular CLI
- **Testing**: Karma & Jasmine
- **Code Quality**: ESLint configuration
- **Package Manager**: npm

### Deployment
- **Target**: Azure Static Web Apps
- **Configuration**: `staticwebapp.config.json` for routing

## Architecture & Structure

### Core Concepts

1. **Partitions**: Multi-tenant system where each club is a separate partition
5. **Authentication**: Auth0-based user authentication with role-based access

## Key Directories

### Frontend
```
src/
├── app/
│   ├── api/                  # Generated API client and models
│   ├── app-engine/           # Core partition and system management
│   ├── core/                 # Core services and guards
│   ├── layout/               # Application layout components
│   ├── modules/admin/        # Main administrative modules
│   └── mock-api/             # Mock API for development
├── @fuse/                    # Fuse template components and utilities
├── environments/             # Environment configurations
└── styles/                   # Global styles and theming
```

### Admin Modules

- **infrastructure**: Shared services (FetchService, NotificationService)

## API Integration

The application uses a generated TypeScript API client (`src/app/api/api.ts`) with the following patterns:

### Query Pattern
- Queries end with `_Query` (e.g., `members_Query`, `membershipFees_Query`)
- Return observables with typed responses
- Include partition filtering for multi-tenancy

### Command Pattern
- Commands end with `_Command` (e.g., `importNewMembers_Command`)
- Used for data mutations
- Return void observables

### Service Layer
- Services extend `FetchService<T>` for consistent data fetching
- Services use reactive patterns with BehaviorSubjects
- Automatic loading states and error handling through base service

## Authentication & Authorization

- **Provider**: Auth0 with domain `clubengine.eu.auth0.com`
- **Audience**: `https://clubengine.ch/api`
- **Storage**: Local storage for tokens
- **Guards**: Auth guards protect administrative routes
- **Roles**: User roles determine access to partitions (Reader, Writer, Admin)

## State Management

### Reactive Patterns
- Services use RxJS observables for state management
- Components use `OnPush` change detection strategy
- Data flows through services with reactive streams

### Partition Management
- `PartitionService` manages selected club context
- All API calls include partition ID for multi-tenancy
- Partition selection persists across sessions

## UI/UX Guidelines

### Component Patterns
- Use Angular Material components consistently
- Implement responsive design with Tailwind CSS
- Follow Fuse template design patterns
- Use `TranslatePipe` for internationalization

### Form Handling
- Reactive forms with Angular FormControl/FormGroup
- Material Design form fields
- Consistent validation patterns

### Data Display
- Card-based layouts for entity display
- Filterable lists with search functionality
- Color-coded membership types
- Responsive grid layouts

## Development Guidelines

### Code Style
- Use standalone components (imports array pattern)
- Implement `OnPush` change detection
- Follow reactive programming patterns
- Use TypeScript strict mode

### Service Patterns
```typescript
// Extend FetchService for data services
export class ExampleService extends FetchService<DataType> {
  constructor(private api: Api, notificationService: NotificationService) {
    super('QueryName', notificationService);
  }
  
  fetch(params): Observable<DataType> {
    return this.fetchItems(this.api.example_Query(params));
  }
}
```

### Component Patterns
```typescript
@Component({
  selector: 'app-example',
  imports: [/* standalone imports */],
  templateUrl: './example.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExampleComponent {
  // Use signals and reactive patterns
  readonly data$ = this.service.data$;
}
```

## Internationalization

- Primary languages: English (en) and German (de)
- Translation files are resx files in backend `ClubEngine.ApiService/Properties`
- Use `TranslatePipe` in templates: `{{ 'KEY' | translate }}`
- Service injection: `TranslateService` for programmatic translations

## Deployment

### Azure Static Web Apps
- Configuration in `staticwebapp.config.json`
- Fallback routing to `index.html` for SPA behavior
- Exclude static assets from fallback routing

### Build Process
- `ng build` for production builds
- Output to `dist/` directory
- Environment-specific configurations

## Common Tasks & Patterns

### Adding a New Admin Module
1. Create module directory under `src/app/modules/admin/`
2. Implement service extending `FetchService`
3. Create component with `OnPush` strategy
4. Add route in `app.routes.ts`
5. Include resolver if needed
6. Add translations for new features

### API Integration
1. Create service for data management
2. Use partition context in API calls
3. Handle loading states through FetchService

### UI Components
1. Use Angular Material components
2. Follow Fuse design patterns
3. Implement responsive design
4. Add proper accessibility attributes
5. Include internationalization support

## Security Considerations

- Auth0 handles authentication securely
- API calls include proper authorization headers
- Partition-based access control
- No sensitive data in client-side storage
- HTTPS enforcement in production

## Performance Guidelines

- Use `OnPush` change detection strategy
- Implement proper unsubscribe patterns
- Lazy load admin modules
- Optimize bundle sizes
- Use trackBy functions for lists
- Implement virtual scrolling for large datasets

## Troubleshooting

### Common Issues
1. **Partition context missing**: Ensure PartitionService has selected partition
2. **Auth token expired**: Check Auth0 token refresh configuration
3. **API errors**: Verify partition access and user permissions
4. **Translation missing**: Check translation keys in i18n files
5. **Material theme issues**: Verify Fuse theme configuration

### Debug Tools
- Angular DevTools for component inspection
- Network tab for API call debugging
- Console for service state logging
- Auth0 debug logs for authentication issues