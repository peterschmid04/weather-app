const apiBaseUrl = (process.env.REACT_APP_API_BASE_URL || "").replace(/\/+$/, "");

export const buildApiUrl = (path) => {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  return `${apiBaseUrl}${normalizedPath}`;
};
