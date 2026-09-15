declare module 'lucide-react';

declare module 'axios' {
  export interface AxiosResponse<T = any> {
    data: T;
  }

  export interface AxiosInstance {
    get<T = any>(url: string): Promise<AxiosResponse<T>>;
    post<T = any>(url: string, body?: any): Promise<AxiosResponse<T>>;
    put<T = any>(url: string, body?: any): Promise<AxiosResponse<T>>;
    delete<T = any>(url: string): Promise<AxiosResponse<T>>;
    interceptors: {
      request: {
        use: (onFulfilled: (config: any) => any, onRejected?: (error: any) => any) => void;
      };
    };
  }

  const axios: {
    create: (cfg?: any) => AxiosInstance;
  };

  export default axios;
}

declare module 'jwt-decode' {
  const jwtDecode: (token: string) => any;
  export default jwtDecode;
}

declare module 'react-router-dom' {
  export function useNavigate(): (to: string) => void;
}

declare module '*.svg' {
  const content: string;
  export default content;
}
